using CringePlugins.Config;
using CringePlugins.Splash;
using NLog;
using Velopack;

namespace CringeLauncher.Stages;

public class CheckUpdatesStage(string[] args, Func<Logger, ValueTask<LauncherConfig?>> readUpdatesConfigAsync) : ILoadingStage
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

        var mgr = new UpdateManager("https://dl.zznty.ru/CringeLauncher/", new()
        {
            AllowVersionDowngrade = true, // in case preview version is higher than stable
            ExplicitChannel = config?.UsePreviewBranch is true ? "win-preview" : null
        });

        // check for new version
        var newVersion = await mgr.CheckForUpdatesAsync();
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

        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion, args);
    }
}