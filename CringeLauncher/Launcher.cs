using System.Reflection;
using System.Runtime.CompilerServices;
using CringeBootstrap.Abstractions;
using CringeLauncher.Utils;
using CringePlugins.Loader;
using CringePlugins.Splash;
using HarmonyLib;
using NLog;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using SpaceEngineers.Game;
using SpaceEngineers.Game.Achievements;
using SpaceEngineers.Game.GUI;
using Velopack;
using VRage;
using VRage.Audio;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.Localization;
using VRage.GameServices;
using VRage.Mod.Io;
using VRage.Platform.Windows;
using VRage.Steam;
using VRage.UserInterface;
using VRageRender;
using VRageRender.ExternalApp;
using Task = System.Threading.Tasks.Task;

namespace CringeLauncher;

public class Launcher : ICorePlugin
{
    private const uint AppId = 244850U;
    private SpaceEngineersGame? _game;
    private readonly Harmony _harmony = new("CringeBootstrap");
    private PluginsLifetime? _lifetime;

    private MyGameRenderComponent? _renderComponent;

    public void Initialize(string[] args)
    {
        if (Type.GetType("GameAnalyticsSDK.Net.Logging.GALogger, GameAnalytics.Mono") is { } gaLoggerType)
            RuntimeHelpers.RunClassConstructor(gaLoggerType.TypeHandle);
        
        LogManager.Setup()
            .LoadConfigurationFromFile()
            .SetupExtensions(s =>
            {
                s.RegisterLayoutRenderer("cringe-exception", e =>
                {
                    if (e.Exception is null) return string.Empty;
                    e.Exception.FormatStackTrace();
                    return e.Exception.ToString();
                });
            });

        LogManager.ReconfigExistingLoggers();
        
        LogManager.GetLogger("CringeBootstrap").Info("Bootstrapping");
        
        //environment variable for viktor's plugins
        Environment.SetEnvironmentVariable("SE_PLUGIN_DISABLE_METHOD_VERIFICATION", "True");
        
        _harmony.PatchAll(typeof(Launcher).Assembly);
        
        #if !DEBUG
            CheckUpdates(args).GetAwaiter().GetResult();
        #endif

        var splash = new Splash();
        
        splash.DefineStage(_lifetime = new PluginsLifetime());
        
        InitTexts();
        SpaceEngineersGame.SetupBasicGameInfo();
        MyFinalBuildConstants.APP_VERSION = MyPerGameSettings.BasicGameInfo.GameVersion.GetValueOrDefault();
        MyShaderCompiler.Init(MyShaderCompiler.TargetPlatform.PC, false);
        MyVRageWindows.Init(MyPerGameSettings.BasicGameInfo.ApplicationName, MySandboxGame.Log,
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                         MyPerGameSettings.BasicGameInfo.ApplicationName), 
                            false, false);
        
        MyPlatformGameSettings.SAVE_TO_CLOUD_OPTION_AVAILABLE = true;
        MyXAudio2.DEVICE_DETAILS_SUPPORTED = false;
        
        if (MyVRage.Platform.System.SimulationQuality == SimulationQuality.Normal)
        {
            MyPlatformGameSettings.SIMPLIFIED_SIMULATION_OVERRIDE = false;
        }

        MyFileSystem.ExePath = Path.GetDirectoryName(args.ElementAtOrDefault(0) ?? Assembly.GetExecutingAssembly().Location)!;
        MyFileSystem.RootPath = new DirectoryInfo(MyFileSystem.ExePath).Parent!.FullName;
        MyInitializer.InvokeBeforeRun(AppId, MyPerGameSettings.BasicGameInfo.ApplicationName,
                                      MyVRage.Platform.System.GetRootPath(), MyVRage.Platform.System.GetAppDataPath(),true, 3, () =>
                                      {
                                          MyFakes.VOICE_CHAT_MIC_SENSITIVITY = MySandboxGame.Config.MicSensitivity;
                                          MyPlatformGameSettings.VOICE_CHAT_AUTOMATIC_ACTIVATION = MySandboxGame.Config.AutomaticVoiceChatActivation;
                                      });
        MyVRage.Platform.Init();
        SpaceEngineersGame.SetupPerGameSettings();
        ConfigureSettings();
        InitThreadPool();
        MyVRage.Platform.System.OnThreadpoolInitialized();
        InitRender();
        
        _renderComponent = new();
        _renderComponent.Start(new(), InitEarlyWindow, MyVideoSettingsManager.Initialize(), MyPerGameSettings.MaxFrameRate);
        _renderComponent.RenderThread.BeforeDraw += MyFpsManager.Update;
        
        // this technically should wait for render thread init, but who cares
        splash.ExecuteLoadingStages();
        
        InitUgc();
        MyFileSystem.InitUserSpecific(MyGameService.UserId.ToString());
        
        _lifetime.RegisterLifetime();
        
        _game = new(args)
        {
            GameRenderComponent = _renderComponent,
            DrawThread = _renderComponent.RenderThread.SystemThread,
            form = MyVRage.Platform.Windows.Window
        };

        _renderComponent.RenderThread.SizeChanged += _game.RenderThread_SizeChanged;
        _renderComponent.RenderThread.UpdateSize();
    }

    public void Run() => _game?.Run();

    private IVRageWindow InitEarlyWindow()
    {
        ImGuiHandler.Instance = new();
        
        MyVRage.Platform.Windows.CreateWindow("Cringe Launcher", MyPerGameSettings.GameIcon, null);

        MyVRage.Platform.Windows.Window.OnExit += MySandboxGame.ExitThreadSafe;
        
        _renderComponent!.RenderThread.UpdateSize();
        MyRenderProxy.RenderThread = _renderComponent.RenderThread;
        
        MyVRage.Platform.Windows.Window.ShowAndFocus();

        return MyVRage.Platform.Windows.Window;
    }

    private async Task CheckUpdates(string[] args)
    {
        var mgr = new UpdateManager("https://dl.zznty.ru/CringeLauncher/");
        
        // check for new version
        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion == null)
            return; // no update available

        // download new version
        await mgr.DownloadUpdatesAsync(newVersion);

        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion, args);
    }

    #region Keen shit

    private static void InitThreadPool()
    {
        ParallelTasks.Parallel.Scheduler = new ThreadPoolScheduler();
        // MySandboxGame.InitMultithreading();
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

    public static void InitUgc()
    {
        var steamGameService = MySteamGameService.Create(false, AppId);
        MyServiceManager.Instance.AddService(steamGameService);
    
        var aggregator = new MyServerDiscoveryAggregator();
        MySteamGameService.InitNetworking(false, steamGameService, MyPerGameSettings.GameName, aggregator);
        // MyEOSService.InitNetworking(false, false, MyPerGameSettings.GameName, steamGameService, "xyza7891964JhtVD93nm3nZp8t1MbnhC",
        //                             "AKGM16qoFtct0IIIA8RCqEIYG4d4gXPPDNpzGuvlhLA", "24b1cd652a18461fa9b3d533ac8d6b5b",
        //                             "1958fe26c66d4151a327ec162e4d49c8", "07c169b3b641401496d352cad1c905d6",
        //                             "https://retail.epicgames.com/", MyEOSService.CreatePlatform(),
        //                             MyPlatformGameSettings.VERBOSE_NETWORK_LOGGING, ArraySegment<string>.Empty, aggregator,
        //                             MyMultiplayer.Channels);
        // EOS networking is disabled due to memory leak, waiting for update with EOSSDK >= 1.15.4
        
        MyServiceManager.Instance.AddService<IMyServerDiscovery>(aggregator);
    
        MyServiceManager.Instance.AddService(MySteamGameService.CreateMicrophone());
    
        MyGameService.WorkshopService.AddAggregate(MySteamUgcService.Create(AppId, steamGameService));

        var modUgc = MyModIoService.Create(MyServiceManager.Instance.GetService<IMyGameService>(), "spaceengineers",
            "264",
            "1fb4489996a5e8ffc6ec1135f9985b5b", "331", "f2b64abe55452252b030c48adc0c1f0e",
            MyPlatformGameSettings.UGC_TEST_ENVIRONMENT, false, MyPlatformGameSettings.MODIO_PLATFORM,
            MyPlatformGameSettings.MODIO_PORTAL);
        modUgc.IsConsentGiven = MySandboxGame.Config.ModIoConsent;
        MyGameService.WorkshopService.AddAggregate(modUgc);
    
        MySpaceEngineersAchievements.Initialize();
    }

    private static void InitRender()
    {
        var renderQualityHint = MyVRage.Platform.Render.GetRenderQualityHint();
        var preset = MyGuiScreenOptionsGraphics.GetPreset(renderQualityHint);
    
        MyRenderProxy.Settings.User = MyVideoSettingsManager
                                      .GetGraphicsSettingsFromConfig(ref preset, renderQualityHint > MyRenderPresetEnum.CUSTOM)
                                      .PerformanceSettings.RenderSettings;
        MyRenderProxy.Settings.EnableAnsel = MyPlatformGameSettings.ENABLE_ANSEL;
        MyRenderProxy.Settings.EnableAnselWithSprites = MyPlatformGameSettings.ENABLE_ANSEL_WITH_SPRITES;
    
        var graphicsRenderer = MySandboxGame.Config.GraphicsRenderer;
        MySandboxGame.Config.GraphicsRenderer = graphicsRenderer;

        _ = new MyEngine();
        MyRenderProxy.Initialize(new MyDX11Render(MyRenderProxy.Settings));
    }

#endregion

    public void Dispose()
    {
        _game?.Dispose();
        MyGameService.ShutDown();
        MyInitializer.InvokeAfterRun();
        MyVRage.Done();
    }
}