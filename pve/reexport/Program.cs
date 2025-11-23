using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace reexport
{
    internal class Program
    {
        static readonly Dictionary<Type, string> structs = new();
        static readonly Dictionary<string, HashSet<Type>> typeMap = new();

        static bool IsStruct(Type type) => type.IsValueType && !type.IsPrimitive && !type.IsEnum && type != typeof(void);

        static void FillStructs(Type type, string assembly)
        {
            // if ((!IsStruct(type) && !type.IsClass && type.StructLayoutAttribute == null) || structs.ContainsKey(type))
            // {
            //     return;
            // }

            // typeMap.TryGetValue(assembly, out var set);
            if (!typeMap.TryGetValue(assembly, out var set))
            {
                set = new();
                typeMap.Add(assembly, set);
            }

            if (!(type.IsClass && (type.StructLayoutAttribute?.Value == LayoutKind.Sequential || type.StructLayoutAttribute?.Value == LayoutKind.Sequential)) && !IsStruct(type))
            {
                return;
            }

            set.Add(type);

            if (structs.ContainsKey(type))
            {
                return;
            }
            
            structs.Add(type, GetCStruct(type));
        }

        static string GetCStruct(Type type)
        {
            StringBuilder sb = new();

            sb.Append($"struct {type.Name} {{\n");
            foreach (var field in type.GetFields())
            {
                try
                {
                    var typeString = GetCTypeString(field.FieldType, []);
                    sb.Append($"\t{typeString} {field.Name};\n");
                }
                catch (KeyNotFoundException) { }
            }
            sb.Append("};");

            return sb.ToString();
        }

        static string GetCTypeString(Type type, HashSet<Type> delegateDeclarations)
        {
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (structs.ContainsKey(type))
            {
                return $"struct {type.Name}";
            }

            if (type == typeof(void))
            {
                return "void";
            }

            if (type == typeof(byte) || type == typeof(sbyte))
            {
                return "char";
            }

            if (type == typeof(short) || type == typeof(ushort) || type == typeof(char))
            {
                return "short";
            }

            if (type == typeof(int) || type == typeof(uint) || type == typeof(bool))
            {
                return "int";
            }

            if (type == typeof(long) || type == typeof(ulong))
            {
                return "long int";
            }

            if (type.IsSubclassOf(typeof(Delegate)))
            {
                delegateDeclarations.Add(type);
                return "void *";
            }

            if (type == typeof(nint) || type == typeof(nuint) || type == typeof(string) || type == typeof(StringBuilder) || type == typeof(SafeHandle) || type.IsByRef || type.IsPointer || type.IsSZArray)
            {
                return "void *";
            }

            if (type == typeof(float))
            {
                return "float";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            throw new KeyNotFoundException();
        }

        static IEnumerable<(MethodInfo, string, string?)> EnumerateNativeMethods(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                {
                    if (method.GetCustomAttribute<DllImportAttribute>() is DllImportAttribute attr)
                    {
                        var assemblyName = Path.GetFileNameWithoutExtension(attr.Value).ToLower().Replace('.', '_');
                        yield return (method, assemblyName, attr.EntryPoint);
                    }
                }
            }
        }

        static void ParametersToString(MethodInfo method, StringBuilder builder, HashSet<Type> delegateDeclarations)
        {
            var parameters = method.GetParameters();
            var first = parameters.FirstOrDefault();
            if (first == null)
            {
                return;
            }

            builder.Append($"{GetCTypeString(first.ParameterType, delegateDeclarations)} {first.Name}");

            foreach (var parameter in parameters.Skip(1))
            {
                builder.Append($", {GetCTypeString(parameter.ParameterType, delegateDeclarations)} {parameter.Name}");
            }
        }
        
        static void CallParametersToString(MethodInfo method, StringBuilder builder)
        {
            var parameters = method.GetParameters();
            if (parameters is []) return;

            builder.AppendJoin(", ", parameters.Select(p =>
            {
                if (p.ParameterType.IsSubclassOf(typeof(Delegate)))
                    return $"_PVE_Trampoline_{p.ParameterType.FullName!.Replace('.', '_').Replace('+', '_')}({p.Name})";
                return p.Name;
            }));
        }

        static void GenerateDelegateTrampolines(HashSet<Type> delegateDeclarations, StringBuilder builder)
        {
            if (delegateDeclarations.Count == 0) return;

            foreach (var del in delegateDeclarations)
            {
                var method = del.GetMethod("Invoke")!;
                var replace = del.FullName!.Replace('.', '_').Replace('+', '_');
                builder.Append($"{GetCTypeString(method.ReturnType, delegateDeclarations)} __attribute__((ms_abi)) _PVE_Stub_{replace}(");
                ParametersToString(method, builder, delegateDeclarations);
                builder.AppendLine(") {");
                builder.AppendLine($"\tprintf(\"callback {replace}\\n\");");
                builder.Append($"\ttypedef {GetCTypeString(method.ReturnType, delegateDeclarations)} (*callback_ptr_t)(");
                ParametersToString(method, builder, delegateDeclarations);
                builder.AppendLine(");");
                builder.Append("\treturn ((callback_ptr_t)cb_userdata_tls)(");
                CallParametersToString(method, builder);
                builder.AppendLine(");");
                builder.AppendLine("}");
                
                builder.AppendLine($"void * _PVE_Trampoline_{replace}(void * ptr) {{");
                builder.AppendLine($"\tvoid* trampoline = callback_make_trampoline(&_PVE_Stub_{replace}, ptr);");
                builder.AppendLine($"\tprintf(\"set callback {replace} - 0x%016lx\\n\", (unsigned long)trampoline);");
                builder.AppendLine("\treturn trampoline;");
                builder.AppendLine("}");
                builder.AppendLine();
            }
        }

        static List<(string, StringBuilder)> GenerateAdapter(Assembly assembly, StreamWriter specWriter, StreamWriter headerWriter)
        {
            foreach ((var method, var name, var _) in EnumerateNativeMethods(assembly))
            {
                foreach (var parameter in method.GetParameters())
                {
                    if (parameter.ParameterType.IsSubclassOf(typeof(Delegate)))
                    {
                        foreach (var info in parameter.ParameterType.GetMethod("Invoke")!.GetParameters())
                        {
                            FillStructs(info.ParameterType, name);
                        }

                        continue;
                    }
                    FillStructs(parameter.ParameterType, name);
                }

                FillStructs(method.ReturnType, name);
            }

            List<(string, StringBuilder)> result = new();

            foreach ((string key, HashSet<Type> set) in typeMap)
            {
                StringBuilder builder = new();

                List<string> methodNames = [];
                HashSet<Type> delegateDeclarations = [];
                
                foreach ((var method, var assm, var entry) in EnumerateNativeMethods(assembly).DistinctBy(b => b.Item3 ?? b.Item1.Name))
                {
                    if (assm != key)
                    {
                        continue;
                    }

                    var name = entry ?? method.Name;

                    methodNames.Add(method.Name);

                    specWriter.Write("@ stdcall ");
                    specWriter.Write("__PVE_");
                    specWriter.Write(name);
                    specWriter.Write('(');
                    var parameterInfos = method.GetParameters();
                    for (var index = 0; index < parameterInfos.Length; index++)
                    {
                        var t = parameterInfos[index].ParameterType;
                        if (t.IsByRef || t.IsPointer || t.IsSubclassOf(typeof(Delegate)) || t == typeof(nint) || t.IsArray || t.IsSZArray)
                            specWriter.Write("ptr");
                        else if (t == typeof(ushort) || t == typeof(short) || t == typeof(uint) || t == typeof(ulong) || t == typeof(int) || t == typeof(long) ||
                                 t == typeof(nuint) || t == typeof(byte) || t == typeof(sbyte) || t == typeof(bool))
                            specWriter.Write("int64");
                        else if (t == typeof(float))
                            specWriter.Write("float");
                        else if (t == typeof(double))
                            specWriter.Write("double");
                        else if (t == typeof(string))
                            specWriter.Write("str");
                        else specWriter.Write("ptr");
                        
                        if (index < parameterInfos.Length - 1)
                            specWriter.Write(' ');
                    }
                    specWriter.Write(") ");
                    specWriter.WriteLine(name);
                    
                    headerWriter.Write($"{GetCTypeString(method.ReturnType, delegateDeclarations)} {name}(");
                    var headerSb = new StringBuilder();
                    ParametersToString(method, headerSb, delegateDeclarations);
                    headerWriter.Write(headerSb);
                    headerWriter.WriteLine(");");
                    
                    builder.Append($"{GetCTypeString(method.ReturnType, delegateDeclarations)} (*__PVE_{name})(");
                    ParametersToString(method, builder, delegateDeclarations);
                    builder.AppendLine(") __attribute__((ms_abi));");

                    builder.Append($"{GetCTypeString(method.ReturnType, delegateDeclarations)} {name}(");
                    ParametersToString(method, builder, delegateDeclarations);
                    builder.AppendLine(") {");
                    builder.AppendLine($"\tprintf(\"invoke {name}\\n\");");
                    builder.Append($"\treturn __PVE_{name}(");
                    CallParametersToString(method, builder);
                    builder.AppendLine(");\n}");
                    builder.AppendLine();
                }

                builder.AppendLine($"char* __{key}_PVEExports[] = {{");
                foreach (var name in methodNames)
                {
                    builder.AppendLine($"\t\"{name}\",");
                }
                builder.AppendLine("\t0");
                builder.AppendLine("};\n");

                var sb = new StringBuilder();

                sb.AppendLine("#include \"callback.h\"");
                sb.AppendLine("#include <stdio.h>");
                sb.AppendLine();
                
                foreach (var type in set)
                {
                    var stru = structs[type];
                    sb.AppendLine(stru);
                    sb.AppendLine();
                    
                    headerWriter.WriteLine(stru);
                    headerWriter.WriteLine();
                }
                
                GenerateDelegateTrampolines(delegateDeclarations, sb);

                sb.AppendLine().Append(builder);

                result.Add((key, sb));
            }


            return result;
        }

        static void Main()
        {
            var assembly = Assembly.LoadFrom("RecastDetourWrapper.dll");
            using var specStream = File.Create(Path.Join("out", "havok.spec"));
            using var specWriter = new StreamWriter(specStream);
            using var headerStream = File.Create(Path.Join("out", "havok.h"));
            using var headerWriter = new StreamWriter(headerStream);
            foreach ((var fname, var content) in GenerateAdapter(assembly, specWriter, headerWriter))
            {
                File.WriteAllText(Path.Combine("out", $"{fname}.c"), content.ToString());
            }
            // File.WriteAllText("out.c", GenerateAdapter(assembly));
        }
    }
}
