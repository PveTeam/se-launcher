using System.Globalization;
using CringeLauncher.CrashPad;
using CringeLauncher.Platform;
using CringeLauncher.Render;
using CringeLauncher.Utils;
using CringePlugins.Splash;
using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using SpaceEngineers.Game;
using VRage;
using VRage.Audio;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.Localization;
using VRageRender;

namespace CringeLauncher.Stages;

internal class PlatformInitializationStage(
    EarlyRenderThread renderThread,
    string? gameDataDirectoryPathOverride,
    CrashPadService crashPadService) : ILoadingStage
{
    public string Name { get; } = "Platform initialization";
    public ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(2);
        progress.Report("Initializing platform");
        
        InitTexts();
        
        SpaceEngineersGame.SetupBasicGameInfo();
        
        MyFinalBuildConstants.APP_VERSION = MyPerGameSettings.BasicGameInfo.GameVersion.GetValueOrDefault();
        
        crashPadService.NextInfo.Version.GameVersion = MyFinalBuildConstants.APP_VERSION.ToString();
        crashPadService.MarkSavePoint();
        
        MyShaderCompiler.Init(MyShaderCompiler.TargetPlatform.PC, false);
        
        MyVRage.Init(new VRageLauncherPlatform(MyPerGameSettings.BasicGameInfo.ApplicationName,
            gameDataDirectoryPathOverride is null
                ? null
                : Path.Join(gameDataDirectoryPathOverride, MyPerGameSettings.BasicGameInfo.ApplicationName),
            renderThread.Surrogate));
        
        MyPlatformGameSettings.SAVE_TO_CLOUD_OPTION_AVAILABLE = true;
        MyXAudio2.DEVICE_DETAILS_SUPPORTED = false;

        if (MyVRage.Platform.System.SimulationQuality == SimulationQuality.Normal)
        {
            MyPlatformGameSettings.SIMPLIFIED_SIMULATION_OVERRIDE = false;
        }
        
        progress.Report("Loading configuration");

        MyInitializer.InvokeBeforeRun(LauncherConstants.AppId, MyPerGameSettings.BasicGameInfo.ApplicationName,
            MyVRage.Platform.System.GetRootPath(), MyVRage.Platform.System.GetAppDataPath(), true, 3, () =>
            {
                if (MySandboxGame.Config.ExperimentalMode)
                {
                    MyPlatformGameSettings.LOBBY_MAX_PLAYERS = 16;
                    MyPlatformGameSettings.LOBBY_TOTAL_PCU_MAX =
                        MyVRage.Platform.System.GetExperimentalPCULimit(100000);
                    MyPlatformGameSettings.SERVER_TOTAL_PCU_MAX = null;
                    MyPlatformGameSettings.OFFLINE_TOTAL_PCU_MAX = MyPlatformGameSettings.LOBBY_TOTAL_PCU_MAX;
                }

                MyFakes.VOICE_CHAT_MIC_SENSITIVITY = MySandboxGame.Config.MicSensitivity;
                MyPlatformGameSettings.VOICE_CHAT_AUTOMATIC_ACTIVATION =
                    MySandboxGame.Config.AutomaticVoiceChatActivation;
            });
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        MyVRage.Platform.Init();
        SpaceEngineersGame.SetupPerGameSettings();
        ConfigureSettings();
        InitThreadPool();
        MyVRage.Platform.System.OnThreadpoolInitialized();
        
        return default;
    }
    
    private static void InitThreadPool()
    {
#if DEBUG
        ParallelTasks.Parallel.Scheduler = new ThreadPoolScheduler();
#else
        MySandboxGame.InitMultithreading();
#endif
    }

    private static void ConfigureSettings()
    {
        MyPlatformGameSettings.ENABLE_LOGOS = false;
    }

    private static void InitTexts()
    {
        var textsPath = Path.Combine(MyFileSystem.RootPath, @"Content\Data\Localization\CoreTexts");
        var hashSet = new HashSet<MyLanguagesEnum>();
        MyTexts.LoadSupportedLanguages(textsPath, hashSet);

        if (!MyTexts.Languages.TryGetValue(MyLanguage.Instance.GetOsLanguageCurrentOfficial(), out var description) &&
            !MyTexts.Languages.TryGetValue(MyLanguagesEnum.English, out description))
            return;

        MyTexts.LoadTexts(textsPath, description.CultureName, description.SubcultureName);
    }
}