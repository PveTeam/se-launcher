using System.Reflection;
using Json.Schema;

namespace CringePlugins.Config.Spec;

public interface IConfigurationSpecProvider
{
    static abstract JsonSchema Spec { get; }

    static JsonSchema? FromType(Type type)
    {
        if (type.IsAssignableTo(typeof(IConfigurationSpecProvider)))
        {
            return (JsonSchema)type.GetProperty(nameof(Spec), BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)!
                .GetValue(null)!;
        }

        return null;
    }
}