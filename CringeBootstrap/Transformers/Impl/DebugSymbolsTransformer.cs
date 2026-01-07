using System.Collections.Immutable;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet.Pdb;
using dnlib.DotNet.Writer;

namespace CringeBootstrap.Transformers.Impl;

internal class DebugSymbolsTransformer(string versionString) : ITransformer
{
    public ImmutableArray<AssemblyName> AcceptedAssemblies { get; } =
    [
        new("HavokWrapper, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("Sandbox.Common, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("Sandbox.Game, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("Sandbox.Graphics, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("SpaceEngineers.Game, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("SpaceEngineers.ObjectBuilders, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Dedicated, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Audio, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.EOS, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Steam, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Game, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Input, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Library, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Math, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Mod.Io, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Network, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Platform.Windows, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Render, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Render11, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.Scripting, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
        new("VRage.UserInterface, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"),
    ];

    public bool Transform(TransformationContext context)
    {
        // creates excess empty files, but also writes codeview data into PE header which is the only thing we need
        var debugHash = MD5.HashData(Encoding.UTF8.GetBytes(versionString + context.Module.Name));
        context.Module.CreatePdbState(PdbFileKind.PortablePDB);
        context.WriterOptions.WritePdb = true;
        context.WriterOptions.PdbFileNameInDebugDirectory = Path.ChangeExtension(context.Module.Name, ".pdb");
        context.WriterOptions.PdbOptions = PdbWriterOptions.None;
        context.WriterOptions.GetPdbContentId = (_, _) => new(new(debugHash), 0); 
        return true;
    }
}