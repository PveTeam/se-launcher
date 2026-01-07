using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using HarmonyLib;

namespace CringeLauncher.Utils;

public static class ExceptionFormatter
{
    internal const string EndOfStackTraceFromPreviousLocation = "   --- End of stack trace from previous location ---";

    private static readonly FrozenDictionary<Type, string> TypeKeywords = FrozenDictionary.ToFrozenDictionary([
        new KeyValuePair<Type, string>(typeof(int), "int"),
        new(typeof(uint), "uint"),
        new(typeof(long), "long"),
        new(typeof(ulong), "ulong"),
        new(typeof(short), "short"),
        new(typeof(ushort), "ushort"),
        new(typeof(float), "float"),
        new(typeof(double), "double"),
        new(typeof(byte), "byte"),
        new(typeof(sbyte), "sbyte"),
        new(typeof(char), "char"),
        new(typeof(bool), "bool"),
        new(typeof(string), "string"),
        new(typeof(object), "object"),
        new(typeof(void), "void")
    ]);

    private static readonly AccessTools.FieldRef<Exception, string?> RemoteStackTraceField =
        AccessTools.FieldRefAccess<Exception, string?>("_remoteStackTraceString");

    internal static readonly AccessTools.FieldRef<Exception, string?> StackTraceField =
        AccessTools.FieldRefAccess<Exception, string?>("_stackTraceString");

    internal static readonly AccessTools.FieldRef<StackFrame, bool> IsLastFrameFromForeignExceptionStackTraceField =
        AccessTools.FieldRefAccess<StackFrame, bool>("_isLastFrameFromForeignExceptionStackTrace");

    public static void FormatStackTrace(this Exception exception)
    {
        ref var remoteStackTraceString = ref RemoteStackTraceField(exception);
        
        if (remoteStackTraceString is not null)
            return;
        
        var stackTrace = new StackTrace(exception, true);

        var sb = new StringBuilder();

        var i = 0;
        while (stackTrace.GetFrame(i++) is { } frame)
        {
            if (!AppendStackFrame(sb, frame, out _)) continue;

            sb.AppendLine();
            
            ref var isLastFrameFromForeignExceptionStackTrace = ref IsLastFrameFromForeignExceptionStackTraceField(frame);
            
            if (isLastFrameFromForeignExceptionStackTrace)
                sb.AppendLine(EndOfStackTraceFromPreviousLocation);
        }
        
        if (sb.Length > 0)
            StackTraceField(exception) = "   --- End of stack trace from the exception formatter ---";
        
        remoteStackTraceString = sb.ToString();
        
        if (exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
                innerException.FormatStackTrace();
            return;
        }

        exception.InnerException?.FormatStackTrace();
    }

    extension(StringBuilder sb)
    {
        internal bool AppendStackFrame(StackFrame frame, [NotNullWhen(true)] out MethodBase? method)
        {
            method = frame.GetMethod();
            if (method is null)
                return false;
            if (method is MethodInfo methodInfo && Harmony.GetOriginalMethod(methodInfo) is { } originalMethod)
                method = originalMethod;

            const string pad = "   ";
            
            sb.Append(pad + "at ");
            sb.AppendMethod(method);

            sb.AppendFileInfo(frame);
            
            sb.AppendPatchInformation(method);
            return true;
        }

        internal StringBuilder AppendPatchInformation(MethodBase method)
        {
            if (Harmony.GetPatchInfo(method) is not { } patchInfo) return sb;
        
            var owners = patchInfo.Owners;
            if (owners.Count == 0) return sb;
        
            sb.Append(" { ");
            for (var i = 0; i < owners.Count; i++)
            {
                sb.Append(owners[i]);
                if (i < owners.Count - 1)
                    sb.Append(", ");
            }
            sb.Append(" }");
        
            return sb;
        }

        private StringBuilder AppendFileInfo(StackFrame frame)
        {
            var fileName = frame.GetFileName();
            if (!string.IsNullOrEmpty(fileName))
            {
                sb.Append(" in ").Append(fileName).Append('(').Append(frame.GetFileLineNumber()).Append(':').Append(frame.GetFileColumnNumber()).Append(')');
            }
        
            return sb;
        }

        public StringBuilder AppendMethod(MethodBase method)
        {
            if (method.DeclaringType is { } declaringType &&
                AssemblyLoadContext.GetLoadContext(declaringType.Assembly) is { } assemblyLoadContext)
                sb.Append(assemblyLoadContext.Name ?? assemblyLoadContext.ToString()).Append("//");

            if (method.IsStatic)
                sb.Append("static ");
        
            void AppendParameter(ParameterInfo parameter)
            {
                if (parameter.ParameterType.IsByRef)
                {
                    if (parameter.IsOut)
                        sb.Append("out ");
                    else if (parameter.IsIn)
                        sb.Append("in ");
                    else
                        sb.Append("ref ");
                    sb.AppendType(parameter.ParameterType.GetElementType()!);
                }
                else
                    sb.AppendType(parameter.ParameterType);
                if (!string.IsNullOrEmpty(parameter.Name))
                    sb.Append(' ').Append(parameter.Name);
            }

            if (method is MethodInfo methodInfo)
                AppendParameter(methodInfo.ReturnParameter);
            else
                sb.Append("new");

            sb.Append(' ');

            if (method.DeclaringType is null)
                sb.Append("<null>");
            else
                sb.AppendType(method.DeclaringType, true);

            if (method is MethodInfo)
            {
                sb.Append('.');
                sb.Append(method.Name);
            }

            if (method.IsGenericMethod)
            {
                if (method.IsGenericMethodDefinition)
                    sb.Append("<?>");
                else
                    sb.AppendGenericArguments(method.GetGenericArguments(), false);
            }

            sb.Append('(');

            var parameters = method.GetParameters();
            for (var j = 0; j < parameters.Length; j++)
            {
                var parameter = parameters[j];
                AppendParameter(parameter);
                if (j < parameters.Length - 1)
                    sb.Append(", ");
            }

            sb.Append(')');
        
            return sb;
        }

        private StringBuilder AppendType(Type type, bool fullName = false)
        {
            void AppendTypeName()
            {
                var span = type.Name.AsSpan();
                var index = span.IndexOf('`');
                if (index != -1) span = span[..index];
                sb.Append(span);
            }

            if (type.IsByRef)
            {
                sb.Append("ref ");
                sb.AppendType(type.GetElementType()!, fullName);
                return sb;
            }
        
            if (type.IsPointer)
            {
                sb.AppendType(type.GetElementType()!, fullName);
                sb.Append('*');
                return sb;
            }
        
            if (type.IsArray)
            {
                sb.AppendType(type.GetElementType()!, fullName);
                sb.Append('[');
                for (var i = 0; i < type.GetArrayRank() - 1; i++)
                    sb.Append(',');
                sb.Append(']');
                return sb;
            }
        
            if (Nullable.GetUnderlyingType(type) is { } underlyingType)
            {
                sb.AppendType(underlyingType, fullName);
                sb.Append('?');
                return sb;
            }

            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(ValueTuple<>))
            {
                sb.Append('(');
                for (var i = 0; i < type.GetGenericArguments().Length; i++)
                {
                    sb.AppendType(type.GetGenericArguments()[i], fullName);
                    if (i < type.GetGenericArguments().Length - 1)
                        sb.Append(", ");
                }
                sb.Append(')');
                return sb;
            }
        
            if (fullName)
            {
                if (!string.IsNullOrEmpty(type.Namespace)) sb.Append(type.Namespace).Append('.');
                AppendTypeName();
            }
            else if (TypeKeywords.TryGetValue(type, out var keyword))
                sb.Append(keyword);
            else
                AppendTypeName();

            if (type.IsGenericType)
            {
                if (type.IsGenericTypeDefinition)
                    sb.Append("<?>");
                else
                    sb.AppendGenericArguments(type.GenericTypeArguments, fullName);
            }

            return sb;
        }

        private StringBuilder AppendGenericArguments(Type[] genericArguments, bool fullName)
        {
            sb.Append('<');
            for (var i = 0; i < genericArguments.Length; i++)
            {
                sb.AppendType(genericArguments[i], fullName);
                if (i < genericArguments.Length - 1)
                    sb.Append(", ");
            }

            sb.Append('>');

            return sb;
        }
    }
}