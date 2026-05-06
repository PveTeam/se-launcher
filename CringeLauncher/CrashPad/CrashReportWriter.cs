using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NLog;

namespace CringeLauncher.CrashPad;

public class CrashReportWriter(CrashInformation information, CrashProcessInformation processInformation)
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public void Write(Stream stream)
    {
        using var writer = new StreamWriter(stream);

        writer.WriteLine("---- CringeLauncher crash report ----");

        writer.Write("// ");
        writer.WriteLine(_wittyStuffs[TimeSpan.FromTicks(Stopwatch.GetTimestamp()).Microseconds % _wittyStuffs.Length]);
        writer.WriteLine();

        if (information.UnhandledException is { } unhandledException)
        {
            writer.Write("Unhandled exception: ");
            var wrotePatchesHead = false;
            WriteFrame(writer, unhandledException.TopFrame, ref wrotePatchesHead);
            writer.WriteLine();
            writer.Write("Original representation: ");
            writer.WriteLine(unhandledException.TopFrame.StringRepresentation);
            if (unhandledException.Thread is not null)
            {
                writer.WriteLine();
                writer.WriteLine("-- Exception Thread Information --");
                writer.Write("Name: ");
                writer.WriteLine(unhandledException.Thread.Name ?? "unnamed");
                writer.Write("ID: ");
                writer.WriteLine(unhandledException.Thread.ManagedId);
                writer.Write("Type: ");
                writer.WriteLine(unhandledException.Thread.Type);
            }
        }
        else
        {
            writer.WriteLine($"No exception information available. Process disappeared? ExitCode: 0x{processInformation.ExitCode:x8}");
        }

        string? stdErrContent;
        if (File.Exists(processInformation.StderrPath))
            try
            {
                stdErrContent = File.ReadAllText(processInformation.StderrPath);
            }
            catch (Exception e)
            {
                stdErrContent = $"Failed to read content{Environment.NewLine}{e}";
            }
        else
            stdErrContent = null;
        if (!string.IsNullOrEmpty(stdErrContent) && stdErrContent != Environment.NewLine)
        {
            writer.WriteLine();
            writer.WriteLine("-- Standard Error Content --");
            writer.WriteLine(stdErrContent);
        }
        
        writer.WriteLine();
        writer.WriteLine("-- Version Information --");
        WriteVersionInfo(writer);
        
        writer.WriteLine();
        writer.WriteLine("-- Plugin Details --");
        WritePluginDetails(writer);

        if (information.ModScripts.Count > 0)
        {
            writer.WriteLine();
            writer.WriteLine("-- Mod Scripts --");
            WriteModDetails(writer);
        }

        writer.WriteLine();
        writer.WriteLine("-- Network Information --");
        WriteNetworkInformation(writer);
        
        writer.WriteLine();
        writer.WriteLine("-- System Details --");
        WriteSystemDetails(writer);
    }

    private void WriteNetworkInformation(StreamWriter writer)
    {
        writer.Write("Check Updates Failed: ");
        writer.WriteLine(information.Network.CheckUpdatesFailed);
        writer.Write("Nuget Source Failed: ");
        writer.WriteLine(information.Network.NugetSourceFailed);
    }

    private static void WriteSystemDetails(StreamWriter writer)
    {
        writer.Write(".NET Version: ");
        writer.WriteLine(RuntimeInformation.FrameworkDescription);
        writer.Write("OS: ");
        writer.WriteLine(RuntimeInformation.OSDescription);
        writer.Write("CPUs: ");
        writer.WriteLine(Environment.ProcessorCount);
        var processorName = QueryProcessorName();
        writer.Write("Processor: ");
        writer.WriteLine(processorName ?? "Unknown");
    }

    internal static string? QueryProcessorName()
    {
        string? processorName = null;
        try
        {
#if WINDOWS
            // im lazy for WMI
            processorName = Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                "ProcessorNameString", null) as string;
#else
            foreach (var line in File.ReadLines("/proc/cpuinfo"))
            {
                if (!line.StartsWith("model name")) continue;
                var separatorIndex = line.IndexOf(':');
                if (separatorIndex > 0)
                    processorName = line[(separatorIndex + 1)..].Trim();
                break;
            }
#endif
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to query processor information");
        }

        return processorName;
    }

    private void WritePluginDetails(StreamWriter writer)
    {
        writer.WriteLine(information.Plugins.Count == 0 ? "No plugins loaded" : "Installed plugins: ");

        foreach (var plugin in information.Plugins)
        {
            var (name, version, source) = plugin;

            if (plugin.Exception is not null)
                writer.Write("[Faulted] ");
            writer.WriteLine($"{name} - {version} ({source})");
        }

        foreach (var plugin in information.Plugins)
        {
            if (plugin.Exception is null) continue;
            
            writer.WriteLine();
            writer.WriteLine($"-- {plugin.Name} Exception --");
            var wrotePatchesHead = false;
            WriteFrame(writer, plugin.Exception, ref wrotePatchesHead);
            writer.WriteLine();
            writer.Write("Original representation: ");
            writer.WriteLine(plugin.Exception.StringRepresentation);
        }
    }

    private void WriteModDetails(StreamWriter writer)
    {
        foreach (var mod in information.ModScripts)
        {
            var (name, cached, exception) = mod;

            if (cached)
                writer.Write("[Cached] ");
            else if (exception is not null)
                writer.Write("[Faulted] ");
            writer.WriteLine(name);
        }
    }

    private void WriteVersionInfo(StreamWriter writer)
    {
        writer.Write("Launcher version: ");
        writer.WriteLine(information.Version.LauncherVersion);
        writer.Write("Updates channel: ");
        writer.WriteLine(information.Version.UpdatesChannel);
        writer.Write("Game Version: ");
        writer.WriteLine(information.Version.GameVersion);
    }

    private static void WriteFrame(StreamWriter writer, ExceptionInformation.ExceptionFrame frame, ref bool wrotePatchesHead)
    {
        var typeName = TypeName.Parse(frame.TypeName);
        writer.Write(typeName.FullName);
        writer.Write(": ");
        writer.WriteLine(frame.Message);
        foreach (var exceptionStackFrame in frame.StackFrames)
        {
            writer.WriteLine(exceptionStackFrame.StringRepresentation);
        }
        
        foreach (var inner in frame.InnerFrames)
        {
            writer.Write("Caused by: ");
            WriteFrame(writer, inner, ref wrotePatchesHead);
        }

        foreach (var stackFrame in frame.StackFrames.Where(b => b.Method?.Patches is { IsEmpty: false }))
        {
            writer.WriteLine();
            
            if (!wrotePatchesHead)
            {
                writer.WriteLine("Patched methods in stack trace:");
                wrotePatchesHead = true;
            }
            
            writer.WriteLine(stackFrame.StringRepresentation);
            foreach (var patch in stackFrame.Method!.Patches)
            {
                writer.Write("    ");
                writer.Write(patch.Type);
                writer.Write(" from ");
                writer.Write(patch.Owner);
                writer.Write(" via ");
                writer.WriteLine(patch.PatchMethod.StringRepresentation);
            }
        }
    }

    private readonly string[] _wittyStuffs =
    [
        "Error: Kerbal not found. Oh wait, wrong universe…",
        "Houston, we have several problems.",
        "Artificial gravity temporarily replaced by artificial stupidity.",
        "Your station's design is now 100% OSHA non-compliant.",
        "Remember: duct tape is cheaper than R&D.",
        "You've just invented the first involuntary wormhole test.",
        "Looks like your thrusters believed in free will.",
        "Computer says no. Also, computer is on fire.",
        "In space, nobody can hear your rage quit.",
        "Congratulations, your reactor just went full Chernobyl 2.0.",
        "Warning: airlock doors and curiosity don't mix.",
        "This was not the docking sequence you were looking for.",
        "Good news: your ship achieved escape velocity. Bad news: without you.",
        "The vacuum appreciates your contribution to entropy.",
        "Your orbital mechanics were more mechanical than orbital.",
        "Somewhere, an engineer just cried in binary.",
        "Don't Panic! Unless you see smoke.",
        "On the bright side, debris fields are great conversation starters.",
        "You've proven once again: mass is hard, physics is harder.",
        "Rebooting… please hold while relativity takes its sweet time.",
        "Relax, even the Millennium Falcon had bad days.",
        "My bad, the quantum spanner slipped again.",
        "Oops, accidentally divided by warp speed.",
        "Turns out black holes aren't good trash compactors.",
        "I'm sorry, the AI thought sarcasm was a valid command.",
        "May the Force be with your navigation system",
        "Why did it have to be snakes? Why not bugs in the guidance system?" ,
        "The truth is out there… somewhere under these logs.",
        "Seems the dilithium supply has a better one-way ticket to chaos.",
    ];
}
