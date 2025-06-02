using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using HarmonyLib;

namespace CringeLauncher.Utils;

public static class ExceptionFormatter
{
    private static readonly AccessTools.FieldRef<Exception, string> StackTraceField = AccessTools.FieldRefAccess<Exception, string>("_remoteStackTraceString");
    
    public static void FormatStackTrace(this Exception exception)
    {
        var stackTrace = new StackTrace(exception, true);
        
        var sb = new StringBuilder();

        var i = 0;
        while (stackTrace.GetFrame(i++) is { } frame)
        {
            var method = frame.GetMethod();
            if (method is null)
                continue;
            
            sb.Append("at ");
            if (method.DeclaringType is { } declaringType &&
                AssemblyLoadContext.GetLoadContext(declaringType.Assembly) is { } assemblyLoadContext)
                sb.Append(assemblyLoadContext).Append("//");
            
            if (method.IsStatic)
                sb.Append("static ");

            if (method is MethodInfo methodInfo)
                sb.Append(methodInfo.ReturnType, false);
            else
                sb.Append("new");
            
            sb.Append(' ');

            if (method.DeclaringType is null)
                sb.Append("<null>");
            else
                sb.Append(method.DeclaringType, true);

            if (method is MethodInfo)
            {
                sb.Append('.');
                sb.Append(method.Name);
            }
            
            if (method.ContainsGenericParameters)
                sb.Append(method.GetGenericArguments(), false);

            sb.Append('(');

            var parameters = method.GetParameters();
            for (var j = 0; j < parameters.Length; j++)
            {
                sb.Append(parameters[j].ParameterType, false);
                if (!string.IsNullOrEmpty(parameters[j].Name))
                    sb.Append(' ').Append(parameters[j].Name);
                if (j < parameters.Length - 1)
                    sb.Append(", ");
            }
            
            sb.Append(')');

            if (frame.GetFileName() is { } fileName)
            {
                sb.Append(" in ").Append(fileName).Append('(').Append(frame.GetFileLineNumber()).Append(':').Append(frame.GetFileColumnNumber()).Append(')');
            }

            sb.AppendLine();
        }

        ref var stackTraceString = ref StackTraceField(exception);
        
        stackTraceString = sb.ToString();
    }

    private static StringBuilder Append(this StringBuilder sb, Type type, bool fullName = false)
    {
        if (fullName && !string.IsNullOrEmpty(type.Namespace))
            sb.Append(type.Namespace).Append('.');
        sb.Append(type.Name);
        if (type.ContainsGenericParameters) 
            sb.Append(type.GetGenericArguments(), fullName);
        
        return sb;
    }

    private static StringBuilder Append(this StringBuilder sb, Type[] genericArguments, bool fullName)
    {
        sb.Append('<');
        for (var i = 0; i < genericArguments.Length; i++)
        {
            sb.Append(genericArguments[i], fullName);
            if (i < genericArguments.Length - 1)
                sb.Append(", ");
        }

        sb.Append('>');

        return sb;
    }
}