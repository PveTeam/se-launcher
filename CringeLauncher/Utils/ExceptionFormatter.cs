using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using HarmonyLib;

namespace CringeLauncher.Utils;

public static class ExceptionFormatter
{
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

    private static readonly AccessTools.FieldRef<Exception, string> StackTraceField =
        AccessTools.FieldRefAccess<Exception, string>("_remoteStackTraceString");
    
    private static readonly AccessTools.FieldRef<StackFrame, bool> IsLastFrameFromForeignExceptionStackTraceField =
        AccessTools.FieldRefAccess<StackFrame, bool>("_isLastFrameFromForeignExceptionStackTrace");

    public static void FormatStackTrace(this Exception exception)
    {
        var stackTrace = new StackTrace(exception, true);

        var sb = new StringBuilder();

        const string pad = "   ";

        var i = 0;
        while (stackTrace.GetFrame(i++) is { } frame)
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;
            if (method is MethodInfo methodInfo && Harmony.GetOriginalMethod(methodInfo) is { } originalMethod)
                method = originalMethod;

            sb.Append(pad + "at ");
            sb.AppendMethod(method);

            sb.AppendFileInfo(frame);
            
            sb.AppendPatchInformation(method);

            sb.AppendLine();
            
            ref var isLastFrameFromForeignExceptionStackTrace = ref IsLastFrameFromForeignExceptionStackTraceField(frame);
            
            if (isLastFrameFromForeignExceptionStackTrace)
                sb.AppendLine("--- End of stack trace from previous location ---");
        }
        
        if (sb.Length > 0)
            sb.AppendLine("--- End of stack trace from the exception formatter ---");

        ref var stackTraceString = ref StackTraceField(exception);

        stackTraceString = sb.ToString();
    }

    private static StringBuilder AppendPatchInformation(this StringBuilder sb, MethodBase method)
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

    private static StringBuilder AppendFileInfo(this StringBuilder sb, StackFrame frame)
    {
        if (frame.GetFileName() is { } fileName)
        {
            sb.Append(" in ").Append(fileName).Append('(').Append(frame.GetFileLineNumber()).Append(':').Append(frame.GetFileColumnNumber()).Append(')');
        }
        
        return sb;
    }

    private static StringBuilder AppendMethod(this StringBuilder sb, MethodBase method)
    {
        if (method.DeclaringType is { } declaringType &&
            AssemblyLoadContext.GetLoadContext(declaringType.Assembly) is { } assemblyLoadContext)
            sb.Append(assemblyLoadContext.Name ?? assemblyLoadContext.ToString()).Append("//");

        if (method.IsStatic)
            sb.Append("static ");

        if (method is MethodInfo methodInfo)
            sb.AppendType(methodInfo.ReturnType, false);
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
            sb.AppendType(parameters[j].ParameterType, false);
            if (!string.IsNullOrEmpty(parameters[j].Name))
                sb.Append(' ').Append(parameters[j].Name);
            if (j < parameters.Length - 1)
                sb.Append(", ");
        }

        sb.Append(')');
        
        return sb;
    }

    private static StringBuilder AppendType(this StringBuilder sb, Type type, bool fullName = false)
    {
        void AppendTypeName()
        {
            var span = type.Name.AsSpan();
            var index = span.IndexOf('`');
            if (index != -1) span = span[..index];
            sb.Append(span);
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

    private static StringBuilder AppendGenericArguments(this StringBuilder sb, Type[] genericArguments, bool fullName)
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