using CringeLauncher.CrashPad;
using CringePlugins.Config;
using CringePlugins.Splash;
using NLog;
using Velopack;
using Velopack.Locators;

namespace CringeLauncher.Stages;

internal class CheckUpdatesStage(
    string[] args,
    Func<Logger, ValueTask<LauncherConfig?>> readUpdatesConfigAsync,
    CrashPadService crashPadService) : ILoadingStage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    public string Name { get; } = "Check updates";
    public async ValueTask Load(ISplashProgress progress)
    {
#if !DEBUG
        await CheckUpdates(Log, progress);
#else
        var config = await readUpdatesConfigAsync(Log);
        Log.Info("Updates disabled: {Flag}", config?.DisableLauncherUpdates ?? false);
#endif
    }
    
    private async Task CheckUpdates(Logger logger, ISplashProgress progress)
    {
        progress.DefineStepsCount(3);
        
        logger.Info("Checking for updates...");
        progress.Report("Checking for updates...");
        
        var config = await readUpdatesConfigAsync(logger);

        var updateOptions = new UpdateOptions
        {
            AllowVersionDowngrade = true, // in case preview version is higher than stable
            ExplicitChannel =
#if WINDOWS
                config?.UsePreviewBranch is true ? "win-preview" : "win"
#else
                config?.UsePreviewBranch is true ? "linux-preview" : "linux"
#endif
        };
        VelopackLocator locator;
        if (OperatingSystem.IsWindows())
            locator = new WindowsVelopackLocator(Path.Join(AppContext.BaseDirectory, "CringeBoostrap.exe"),
                (uint)Environment.ProcessId, null);
        else if (OperatingSystem.IsLinux())
            locator = new LinuxVelopackLocator(Path.GetFullPath(Path.Join(AppContext.BaseDirectory, "../../bin", "CringeBoostrap")),
                (uint)Environment.ProcessId, null);
        else throw new PlatformNotSupportedException();
        var mgr = new UpdateManager(config?.UpdatesSource ?? LauncherConfigRegionalDefaults.Current.UpdatesSource, updateOptions,
            locator
            );

        if (mgr.CurrentVersion != null)
            crashPadService.NextInfo.Version.LauncherVersion = mgr.CurrentVersion.ToFullString();
        crashPadService.NextInfo.Version.UpdatesChannel = updateOptions.ExplicitChannel;

        // check for new version
        UpdateInfo? newVersion;
        try
        {
            newVersion = await mgr.CheckForUpdatesAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to check for updates");
            crashPadService.NextInfo.Network.CheckUpdatesFailed = true;
            return;
        }
        finally
        {
            crashPadService.MarkSavePoint();
        }
        if (newVersion == null)
        {
            logger.Info("Up to date");
            return; // no update available
        }

        // print update info
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"New version available: {mgr.CurrentVersion} -> {newVersion.TargetFullRelease.Version}");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine();
        if (!string.IsNullOrEmpty(newVersion.TargetFullRelease.NotesMarkdown))
        {
            Console.WriteLine(newVersion.TargetFullRelease.NotesMarkdown);
            Console.WriteLine();
        }
        Console.ResetColor();

        if (config?.DisableLauncherUpdates is true)
        {
            logger.Warn("Updates Disabled, skipping update");
            return;
        }

        logger.Info("Downloading new version...");
        progress.Report("Downloading new version...");

        // download new version
        await mgr.DownloadUpdatesAsync(newVersion, p => progress.Report(p / 100f));

        logger.Info("Done! Restarting...");
        progress.Report("Done! Restarting...");
        
        // reset entrypoint
        Environment.SetEnvironmentVariable("DOTNET_BOOTSTRAP_ENTRYPOINT", null);

        var newArgs = args;
        var index = args.AsSpan().IndexOf("--crashpad-stderr-redirect");
        if (index != -1)
            // remove --crashpad-stderr-redirect and path
            newArgs = [..args.AsSpan(..index), ..args.AsSpan(index + 2)];

        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion, newArgs);
    }
}
