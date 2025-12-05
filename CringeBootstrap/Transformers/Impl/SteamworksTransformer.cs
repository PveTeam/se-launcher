using System.Collections.Immutable;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using HarmonyLib;
using NLog;
using Steamworks;

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

        method = typeDefinition.FindMethod("GetAuthSessionTicket");

        if (method is not null) PatchAuthTicket(method);

        return true;
    }

    private static void PatchAuthTicket(MethodDef method)
    {
        var instructions = method.Body.Instructions;

        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            
            if (instruction.OpCode == OpCodes.Call && instruction.Operand is IMethodDefOrRef operand &&
                operand.Name == "GetAuthSessionTicket" && operand.DeclaringType.Name == "SteamUser")
            {
                instruction.Operand =
                    method.Module.Import(AccessTools.DeclaredMethod(typeof(SteamUser),
                        nameof(SteamUser.GetAuthSessionTicket)));

                var local = new Local(method.Module.ImportAsTypeSig(typeof(SteamNetworkingIdentity)));
                method.Body.Variables.Add(local);
                
                instructions.Insert(i, Instruction.Create(OpCodes.Ldloca, local));

                instructions.Insert(0,
                    Instruction.Create(OpCodes.Call,
                        method.Module.Import(AccessTools.DeclaredMethod(typeof(SteamNetworkingIdentity),
                            nameof(SteamNetworkingIdentity.SetLocalHost)))));
                instructions.Insert(0, Instruction.Create(OpCodes.Ldloca, local));
                
                break;
            }
        }
    }

    private static bool Remove(MethodDef method)
    {
        var instructions = method.Body.Instructions;
        var index = -1;
        for (var i = 0; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if (instruction.OpCode == OpCodes.Call && instruction.Operand is IMethodDefOrRef operand &&
                operand.Name == "RequestCurrentStats" && operand.DeclaringType.Name == nameof(SteamUserStats))
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