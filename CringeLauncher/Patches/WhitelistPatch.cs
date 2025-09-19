using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.RegularExpressions;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using VRage.FileSystem;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptWhitelist), MethodType.Constructor, typeof(MyScriptCompiler))]
public static class WhitelistPatch
{
    private static void Prefix(MyScriptCompiler scriptCompiler)
    {
        var baseDir = new FileInfo(typeof(Type).Assembly.Location).DirectoryName!;

        string[] gameAssemblies =
        [
            Path.Join(MyFileSystem.ExePath, "ProtoBuf.Net.dll"),
            Path.Join(MyFileSystem.ExePath, "ProtoBuf.Net.Core.dll"),
            Path.Join(MyFileSystem.ExePath, "Sandbox.Game.dll"),
            Path.Join(MyFileSystem.ExePath, "Sandbox.Common.dll"),
            Path.Join(MyFileSystem.ExePath, "Sandbox.Graphics.dll"),
            Path.Join(MyFileSystem.ExePath, "VRage.dll"),
            Path.Join(MyFileSystem.ExePath, "VRage.Library.dll"),
            Path.Join(MyFileSystem.ExePath, "VRage.Math.dll"),
            Path.Join(MyFileSystem.ExePath, "VRage.Game.dll"),
            Path.Join(MyFileSystem.ExePath, "VRage.Render.dll"),
            Path.Join(MyFileSystem.ExePath, "VRage.Input.dll"),
            Path.Join(MyFileSystem.ExePath, "VRage.Scripting.dll"),
            Path.Join(MyFileSystem.ExePath, "SpaceEngineers.ObjectBuilders.dll"),
            Path.Join(MyFileSystem.ExePath, "SpaceEngineers.Game.dll")
        ];

        scriptCompiler.AddReferencedAssemblies(
            [
                typeof(Type).Assembly.Location,
                typeof(LinkedList<>).Assembly.Location,
                typeof(Regex).Assembly.Location,
                typeof(Enumerable).Assembly.Location,
                typeof(ConcurrentBag<>).Assembly.Location,
                typeof(ImmutableArray).Assembly.Location,
                typeof(PropertyChangedEventArgs).Assembly.Location,
                typeof(TypeConverter).Assembly.Location,
                typeof(System.Diagnostics.TraceSource).Assembly.Location,
                typeof(System.Security.Policy.Evidence).Assembly.Location,
                Path.Join(baseDir, "System.Xml.ReaderWriter.dll"),
                Path.Join(baseDir, "netstandard.dll"),
                Path.Join(baseDir, "System.Runtime.dll"),
                ..gameAssemblies
            ]
        );
        
        ScriptCompilationSettingsPatch.CompilerMetadataReferences.UnionWith(Basic.Reference.Assemblies.Net90.References.All);
        ScriptCompilationSettingsPatch.CompilerMetadataReferences.UnionWith(
            gameAssemblies.Select(b => MetadataReference.CreateFromFile(b)));
    }
}