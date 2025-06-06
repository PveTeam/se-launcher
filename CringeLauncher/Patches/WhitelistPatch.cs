using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.RegularExpressions;
using HarmonyLib;
using VRage.FileSystem;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptWhitelist), MethodType.Constructor, typeof(MyScriptCompiler))]
public static class WhitelistPatch
{
    private static void Prefix(MyScriptCompiler scriptCompiler)
    {
        var baseDir = new FileInfo(typeof(Type).Assembly.Location).DirectoryName!;

        scriptCompiler.AddReferencedAssemblies(
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
            Path.Combine(baseDir, "System.Xml.ReaderWriter.dll"),
            Path.Combine(MyFileSystem.ExePath, "ProtoBuf.Net.dll"),
            Path.Combine(MyFileSystem.ExePath, "ProtoBuf.Net.Core.dll"),
            Path.Combine(baseDir, "netstandard.dll"),
            Path.Combine(baseDir, "System.Runtime.dll"),
            Path.Combine(MyFileSystem.ExePath, "Sandbox.Game.dll"),
            Path.Combine(MyFileSystem.ExePath, "Sandbox.Common.dll"),
            Path.Combine(MyFileSystem.ExePath, "Sandbox.Graphics.dll"),
            Path.Combine(MyFileSystem.ExePath, "VRage.dll"),
            Path.Combine(MyFileSystem.ExePath, "VRage.Library.dll"),
            Path.Combine(MyFileSystem.ExePath, "VRage.Math.dll"),
            Path.Combine(MyFileSystem.ExePath, "VRage.Game.dll"),
            Path.Combine(MyFileSystem.ExePath, "VRage.Render.dll"),
            Path.Combine(MyFileSystem.ExePath, "VRage.Input.dll"),
            Path.Combine(MyFileSystem.ExePath, "SpaceEngineers.ObjectBuilders.dll"),
            Path.Combine(MyFileSystem.ExePath, "SpaceEngineers.Game.dll"));
    }
}