using dnlib.DotNet;

namespace CringeBootstrap.CrossGen;

internal record GameCacheKey(string Value)
{
    private const string CacheKeyFileName = "SpaceEngineers.Game.dll";
    
    public static GameCacheKey FromDirectory(string dir)
    {
        var definition = ModuleDefMD.Load(Path.Join(dir, CacheKeyFileName));
        
        var typeDef = definition.FindReflection("SpaceEngineers.Game.SpaceEngineersGame");
        var versionNumber = (int)typeDef.GetField("SE_VERSION").Constant.Value;
        var buildNumber = (int)typeDef.GetField("CLIENT_BUILD_NUMBER").Constant.Value;

        var value = VersionToString(versionNumber);

        if (buildNumber > 0)
            value += $"b{buildNumber}";

        return new(value);
    }

    private static string VersionToString(int version)
    {
        const int charsCount = 1 + 3 + 3;
        var text = version.ToString();
        if (text.Length <= charsCount)
            text = new string('0', charsCount - text.Length) + text;
        return new Version($"{text[..1]}.{text.Substring(1, 3)}.{text.Substring(4, 3)}").ToString();
    }
}