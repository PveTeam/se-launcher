using CringeBootstrap.Abstractions;
using CringeLauncher.Utils;
using CringePlugins.Config;
using CringePlugins.Loader;
using CringePlugins.Render;
using CringePlugins.Services;
using CringePlugins.Splash;
using Epic.OnlineServices.VRage;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Polly;
using Polly.Extensions.Http;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using SpaceEngineers.Game;
using SpaceEngineers.Game.Achievements;
using SpaceEngineers.Game.GUI;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
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
    private readonly string? _gameDataDirectoryPathOverride;
    protected const uint AppId = 244850U;
    private SpaceEngineersGame? _game;
    private readonly Harmony _harmony = new("CringeBootstrap");
    private IPluginsLifetime? _lifetime;

    private MyGameRenderComponent? _renderComponent;

    private readonly DirectoryInfo _configDir;
    private readonly DirectoryInfo _dir;

    public Launcher() : this(null) { }

    protected Launcher(string? gameDataDirectoryPathOverride)
    {
        _gameDataDirectoryPathOverride = gameDataDirectoryPathOverride;
        _dir = Directory.CreateDirectory(Path.Join(
            gameDataDirectoryPathOverride ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CringeLauncher"));
        _configDir = _dir.CreateSubdirectory("config");
    }

    public bool Initialize(string[] args)
    {
        if (Type.GetType("GameAnalyticsSDK.Net.Logging.GALogger, GameAnalytics.Mono") is { } gaLoggerType)
            RuntimeHelpers.RunClassConstructor(gaLoggerType.TypeHandle);

        LogManager.Setup()
            .SetupExtensions(s =>
            {
                s.RegisterLayoutRenderer("cringe-exception", e =>
                {
                    if (e.Exception is null) return string.Empty;
                    e.Exception.FormatStackTrace();
                    return e.Exception.ToString();
                });
            })
            .LoadConfigurationFromFile(optional: false);

        LogManager.ReconfigExistingLoggers();

        var logger = LogManager.GetLogger("CringeBootstrap");
        logger.Info("Bootstrapping");

        //environment variable for viktor's plugins
        Environment.SetEnvironmentVariable("SE_PLUGIN_DISABLE_METHOD_VERIFICATION", "True");

#if !DEBUG
        CheckUpdates(args, logger).GetAwaiter().GetResult();
#else
        logger.Info("Updates disabled: {Flag}", CheckUpdatesDisabledAsync(logger).GetAwaiter().GetResult());
#endif

        // hook up steam as we ship it inside base context as an override
        if (AssemblyLoadContext.GetLoadContext(typeof(Launcher).Assembly) is ICoreLoadContext coreLoadContext)
            NativeLibrary.SetDllImportResolver(typeof(Steamworks.Constants).Assembly, (name, _, _) => coreLoadContext.ResolveUnmanagedDll(name));
        NativeLibrary.SetDllImportResolver(typeof(EosService).Assembly, (name, _, _) => NativeLibrary.Load(Path.Join(AppContext.BaseDirectory, name)));

        _harmony.PatchAll(typeof(Launcher).Assembly);

        MyFileSystem.ExePath = Path.GetDirectoryName(args.ElementAtOrDefault(0) ?? Assembly.GetExecutingAssembly().Location)!;
        MyFileSystem.RootPath = new DirectoryInfo(MyFileSystem.ExePath).Parent!.FullName;

        var serviceProvider = SetupServices();
        
        var splash = new Splash();

        splash.DefineStage(_lifetime = serviceProvider.GetRequiredService<IPluginsLifetime>());

        InitTexts();
        SpaceEngineersGame.SetupBasicGameInfo();
        MyFinalBuildConstants.APP_VERSION = MyPerGameSettings.BasicGameInfo.GameVersion.GetValueOrDefault();
        MyShaderCompiler.Init(MyShaderCompiler.TargetPlatform.PC, false);
        MyVRageWindows.Init(MyPerGameSettings.BasicGameInfo.ApplicationName, MySandboxGame.Log,
                            Path.Join(_gameDataDirectoryPathOverride ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                         MyPerGameSettings.BasicGameInfo.ApplicationName),
                            false, false);

        MyPlatformGameSettings.SAVE_TO_CLOUD_OPTION_AVAILABLE = true;
        MyXAudio2.DEVICE_DETAILS_SUPPORTED = false;

        if (MyVRage.Platform.System.SimulationQuality == SimulationQuality.Normal)
        {
            MyPlatformGameSettings.SIMPLIFIED_SIMULATION_OVERRIDE = false;
        }

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

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        _renderComponent = new();
        _renderComponent.Start(new(), () => InitEarlyWindow(splash), MyVideoSettingsManager.Initialize(), MyPerGameSettings.MaxFrameRate);
        _renderComponent.RenderThread.BeforeDraw += MyFpsManager.Update;

        // this technically should wait for render thread init, but who cares
        splash.ExecuteLoadingStages();

        InitUgc();
        MyFileSystem.InitUserSpecific(MyGameService.UserId.ToString());

        _lifetime.RegisterLifetime();

        WaitForDevice();

        _game = new(args)
        {
            GameRenderComponent = _renderComponent,
            DrawThread = _renderComponent.RenderThread.SystemThread,
            form = MyVRage.Platform.Windows.Window
        };

        _renderComponent.RenderThread.SizeChanged += _game.RenderThread_SizeChanged;
        _renderComponent.RenderThread.UpdateSize();
        
        return true;
    }

    public bool Run()
    {
        try
        {
            _game?.Run();
        }
        catch (Exception e)
        {
            LogManager.GetLogger("Game").Fatal(e, "Fatal exception in game loop");
            return false;
        }
        
        return true;
    }


    private IServiceProvider SetupServices()
    {
        var services = new ServiceCollection();

        var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(1));

        services.AddHttpClient<PluginsLifetime, PluginsLifetime>((client, provider) => 
                new PluginsLifetime(provider.GetRequiredService<ConfigHandler>(), client, _dir))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            })
            .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<ImGuiImageService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            })
            .AddPolicyHandler(retryPolicy);

        services.AddSingleton(_ => RenderHandler.Current)
            .AddSingleton<IPluginsLifetime>(s => s.GetRequiredService<PluginsLifetime>())
            .AddSingleton<IImGuiImageService>(s => s.GetRequiredService<ImGuiImageService>())
            .AddSingleton(_ => new ConfigHandler(_configDir));

        return GameServicesExtension.GameServices = services.BuildServiceProvider();
    }

    private void WaitForDevice()
    {
        if (_renderComponent!.RenderThread.CurrentSettings.DRSSettingsPresets is not null)
            return;

        var resetEvent = new ManualResetEventSlim(false);

        void RenderThreadOnSizeChanged(int width, int height, MyViewport viewport)
        {
            _renderComponent.RenderThread.SizeChanged -= RenderThreadOnSizeChanged;
            resetEvent.Set();
        }

        _renderComponent.RenderThread.SizeChanged += RenderThreadOnSizeChanged;

        resetEvent.Wait();
    }

    private IVRageWindow InitEarlyWindow(Splash splash)
    {
        ImGuiHandler.Instance = new(_configDir);

        RenderHandler.Current.RegisterComponent(splash);

        MyVRage.Platform.Windows.CreateWindow("Cringe Launcher", MyPerGameSettings.GameIcon, null);

        MyVRage.Platform.Windows.Window.OnExit += MySandboxGame.ExitThreadSafe;

        MyRenderProxy.RenderThread = _renderComponent!.RenderThread;

        MyVRage.Platform.Windows.Window.ShowAndFocus();

        return MyVRage.Platform.Windows.Window;
    }

    protected virtual async Task<bool> CheckUpdatesDisabledAsync(Logger logger)
    {
        var path = Path.Join(_configDir.FullName, "launcher.json");

        if (!File.Exists(path))
            return false;

        try
        {
            await using var stream = File.OpenRead(path);

            var conf = await JsonSerializer.DeserializeAsync<LauncherConfig>(stream, ConfigHandler.SerializerOptions);

            return conf?.DisableLauncherUpdates ?? false;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error reading launcher config");
        }

        return false;
    }

    private async Task CheckUpdates(string[] args, Logger logger)
    {
        logger.Info("Checking for updates...");

        var mgr = new UpdateManager("https://dl.zznty.ru/CringeLauncher/");

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

        if (await CheckUpdatesDisabledAsync(logger))
        {
            logger.Warn("Updates Disabled, skipping update");
            return;
        }

        logger.Info("Downloading new version...");

        // download new version
        await mgr.DownloadUpdatesAsync(newVersion);

        logger.Info("Done! Restarting...");

        // install new version and restart app
        mgr.ApplyUpdatesAndRestart(newVersion, args);
    }

    #region Keen shit

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

    protected virtual void InitUgc()
    {
        var steamGameService = MySteamGameService.Create(false, AppId);
        MyServiceManager.Instance.AddService(steamGameService);

        var aggregator = new MyServerDiscoveryAggregator();
        MySteamGameService.InitNetworking(false, steamGameService, MyPerGameSettings.GameName, aggregator);
        EosService.InitNetworking(false, false, MyPerGameSettings.GameName, steamGameService, "xyza7891964JhtVD93nm3nZp8t1MbnhC",
                                    "AKGM16qoFtct0IIIA8RCqEIYG4d4gXPPDNpzGuvlhLA", "24b1cd652a18461fa9b3d533ac8d6b5b",
                                    "1958fe26c66d4151a327ec162e4d49c8", "07c169b3b641401496d352cad1c905d6",
                                    "https://retail.epicgames.com/", EosService.CreatePlatform(),
                                    MyPlatformGameSettings.VERBOSE_NETWORK_LOGGING, [], aggregator,
                                    MyMultiplayer.Channels);

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