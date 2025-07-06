using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;

namespace CringePlugins.MSBuild;

public class GenerateRunConfig : Task
{
    [Required] public required string RunConfigPath { get; set; }

    [Required] public required string ProjectName { get; set; }

    private const string RunConfigTemplate = """
                                             {
                                               "$schema": "http://json.schemastore.org/launchsettings.json",
                                               "profiles": {
                                                 "$projectName$ Dev Client": {
                                                   "commandName": "Executable",
                                                   "executablePath": "BootstrapExecutablePathPlaceholder",
                                                   "commandLineArgs": "\"GameExecutablePathPlaceholder\"",
                                                   "environmentVariables": {
                                                     "DOTNET_BOOTSTRAP_ENTRYPOINT": "CringeLauncher.UserDev.UserDevLauncher, CringeLauncher",
                                                     "DOTNET_USERDEV_RUNDIR": "data",
                                                     "DOTNET_USERDEV_PLUGINDIR": "."
                                                   }
                                                 }
                                               }
                                             }
                                             """;

    public override bool Execute()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            Log.LogError("Only windows is supported");
            return false;
        }
        
        // branch off so jit wont notice in case Win32 package is missing from sdk distribution on non-windows platforms
        // too lazy to test if it actually is missing
        return ExecuteInternal();
    }

    private bool ExecuteInternal()
    {
        string? GetInstallLocation(RegistryKey baseKey, string subKey)
        {
            using var key = baseKey.OpenSubKey(subKey);
            return key?.GetValue("InstallLocation") as string;
        }

        var gamePath = GetInstallLocation(Registry.LocalMachine,
            @"Software\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 244850");
        var bootstrapPath = GetInstallLocation(Registry.CurrentUser,
            @"Software\Microsoft\Windows\CurrentVersion\Uninstall\CringeLauncher");

        if (string.IsNullOrEmpty(gamePath))
        {
            Log.LogError("Failed to find Space Engineers install location");
            return false;
        }
        
        if (string.IsNullOrEmpty(bootstrapPath))
        {
            Log.LogError("Failed to find CringeLauncher install location");
            return false;
        }
        
        gamePath = Path.Combine(gamePath, @"Bin64\SpaceEngineers.exe").Replace(@"\", @"\\");
        bootstrapPath = Path.Combine(bootstrapPath, @"current\CringeBootstrap.exe").Replace(@"\", @"\\");

        var runConfigText = RunConfigTemplate
            .Replace("BootstrapExecutablePathPlaceholder", bootstrapPath)
            .Replace("GameExecutablePathPlaceholder", gamePath)
            .Replace("$projectName$", ProjectName);
        
        var runConfigDir = Path.GetDirectoryName(RunConfigPath);
        if (runConfigDir is not null && !Directory.Exists(runConfigDir))
            Directory.CreateDirectory(runConfigDir);

        File.WriteAllText(RunConfigPath, runConfigText);
        return true;
    }
}