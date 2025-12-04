using System.Collections.Immutable;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NLog;

namespace CringeBootstrap.Transformers.Impl;

internal class SteamworksTransformer : ITransformer
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public ImmutableArray<AssemblyName> AcceptedAssemblies { get; } =
        [new("VRage.Steam, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")];

    public bool Transform(ModuleDefMD moduleDefinition)
    {
        var typeDefinition = moduleDefinition.Find("VRage.Steam.MySteamService", true);
        if (typeDefinition is null) return false;

        var ctor = typeDefinition.FindInstanceConstructors().FirstOrDefault();

        if (ctor is null)
        {
            Log.Warn("Failed to find steam service ctor");
            return false;
        }
        
        if (!Remove(ctor)) return false;

        var method = typeDefinition.FindMethod("LoadStats");
        
        if (method is null)
        {
            Log.Warn("Failed to find steam service LoadStats");
            return false;
        }
        
        if (!Remove(method)) return false;

        return true;
    }

    private static bool Remove(MethodDef method)
    {
        var instructions = method.Body.Instructions;
        var index = -1;
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if (instruction.OpCode == OpCodes.Call && instruction.Operand is IMethodDefOrRef operand &&
                operand.Name == "RequestCurrentStats" && operand.DeclaringType.Name == "SteamUserStats")
            {
                index = i;
            }
        }

        if (index == -1)
        {
            Log.Warn("Failed to find RequestCurrentStats call");
            return false;
        }
        
        instructions[index] = Instruction.Create(OpCodes.Nop); // call
        instructions[index + 1] = Instruction.Create(OpCodes.Nop); // pop
        return true;
    }
}