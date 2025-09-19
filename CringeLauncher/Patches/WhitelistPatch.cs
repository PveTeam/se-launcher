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
        
        scriptCompiler.m_metadataReferences.Clear();
        scriptCompiler.m_metadataReferences.AddRange(Basic.Reference.Assemblies.Net90.References.All);
        scriptCompiler.m_metadataReferences.AddRange(gameAssemblies.Select(b => MetadataReference.CreateFromFile(b)));
    }
}