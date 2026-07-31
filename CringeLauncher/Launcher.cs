using CringeBootstrap.Abstractions;
using CringeLauncher.CrashPad;
using CringeLauncher.Patches;
using CringeLauncher.Render;
using CringeLauncher.Stages;
using CringeLauncher.Utils;
using CringePlugins.Config;
using CringePlugins.Loader;
using CringePlugins.Render;
using CringePlugins.Services;
using CringePlugins.Splash;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Polly;
using Polly.Extensions.Http;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Game;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CringeLauncher.Services;
using CringePlugins.Abstractions;
using Velopack;
using VRage;
using VRage.Audio;
using VRage.FileSystem;
using VRageRender;
using Windows.Win32;
using Windows.Win32.System.Console;
using CringeLauncher.Platform;
using CringeLauncher.Platform.Xplat;
using SharedCringe.Utils;

namespace CringeLauncher;

public class Launcher : ICorePlugin
{
    public bool RestartRequested { get; private set; }

    private readonly string? _gameDataDirectoryPathOverride;
    private SpaceEngineersGame? _game;
    private IPluginsLifetime? _lifetime;

    private readonly DirectoryInfo _configDir;
    private readonly DirectoryInfo _dir;
    private EarlyRenderThread? _renderThread;
    private CrashPadService? _crashPadService;
    
    protected virtual bool IsDedicated => false;

    public Launcher() : this(null) { }

    protected Launcher(string? gameDataDirectoryPathOverride)
    {
        _gameDataDirectoryPathOverride = gameDataDirectoryPathOverride;
        _dir = Directory.CreateDirectory(Path.Join(
            gameDataDirectoryPathOverride ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CringeLauncher"));
        _configDir = _dir.CreateSubdirectory("config");
    }

    public bool Initialize(string[] args, ServiceCollection services)
    {
        RestartRequested = false;
        var stdErrRedirectIndex = args.IndexOf("--crashpad-stderr-redirect");
        if (stdErrRedirectIndex != -1 && !ConsoleHandler.ShouldKeepConsole(args))
        {
            var redirectPath = args[stdErrRedirectIndex + 1];
            ConsoleHandler.RedirectStandardError(redirectPath);
        }

        if (Type.GetType("GameAnalyticsSDK.Net.Logging.GALogger, GameAnalytics.Mono") is { } gaLoggerType)
            RuntimeHelpers.RunClassConstructor(gaLoggerType.TypeHandle);

        NLogLogging.Init();

        var logger = LogManager.GetLogger("CringeLauncher");

        _crashPadService = new CrashPadService();

        var serviceProvider = SetupServices(services);

        if (!IsDedicated)
        {
#if WINDOWS
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            ImGuiHandler.Instance = new Render.Win.WinImGuiHandler(_configDir);
#else
            new HarmonyLib.Harmony("CringeBootstrap").PatchCategory(typeof(Launcher).Assembly, "EarlyRender");
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                throw new PlatformNotSupportedException("Platforms other than linux are not supported");
            ImGuiHandler.Instance = new Render.Xplat.XplatImGuiHandler(_configDir);
#endif
        
            _renderThread = new EarlyRenderThread(ConsoleHandler.ShouldKeepConsole(args));
        }
        else RenderHandler.InitializeNoop();
        
        using var splash = new Splash();
        RenderHandler.Current.RegisterComponent(splash);

        splash.DefineStage(new CheckUpdatesStage(args, ReadUpdateConfigAsync, _crashPadService));
        splash.DefineStage(new LauncherPatchesStage());

        //environment variable for viktor's plugins
        Environment.SetEnvironmentVariable("SE_PLUGIN_DISABLE_METHOD_VERIFICATION", "True");

        // hook up steam as we ship it inside base context as an override
        if (AssemblyLoadContext.GetLoadContext(typeof(Launcher).Assembly) is ICoreLoadContext coreLoadContext)
            NativeLibrary.SetDllImportResolver(typeof(Steamworks.Constants).Assembly, (name, _, _) => coreLoadContext.ResolveUnmanagedDll(name));
#if WINDOWS
        NativeLibrary.SetDllImportResolver(typeof(Epic.OnlineServices.VRage.EosService).Assembly, (name, _, _) => NativeLibrary.Load(Path.Join(AppContext.BaseDirectory, name)));
#endif

        if (splash.ExecuteLoadingStages() is { } preInitException)
        {
            logger.Fatal("Failed to run pre-init stages");
            _crashPadService?.CaptureCurrentThreadException(preInitException);
            return false;
        }

        MyFileSystem.ExePath =
            Path.GetDirectoryName(args.ElementAtOrDefault(0) ?? Assembly.GetExecutingAssembly().Location)!;
        MyFileSystem.RootPath = new DirectoryInfo(MyFileSystem.ExePath).Parent!.FullName;

        splash.DefineStage(new PlatformInitializationStage(_renderThread, _gameDataDirectoryPathOverride,
            _crashPadService));
        splash.DefineStage(new RenderInitializationStage(_renderThread));
        splash.DefineStage(_lifetime = serviceProvider.GetRequiredService<IPluginsLifetime>());

        Initialize(splash);

        // this technically should wait for render thread init, but who cares
        if (splash.ExecuteLoadingStages() is { } initException)
        {
            logger.Fatal("Failed to run init stages");
            _crashPadService?.CaptureCurrentThreadException(initException);
            return false;
        }

        MyFileSystem.InitUserSpecific(IsDedicated ? null : MyGameService.UserId.ToString());

        _lifetime.RegisterLifetime();
        ImGuiHandler.Instance.NotifyPluginsLoaded();
        _crashPadService.PullPluginInfo((PluginsLifetime)_lifetime);
        
        GameReadyHandlerPatch.GameReady += () => _crashPadService.PullPluginInfo((PluginsLifetime)_lifetime);

        _renderThread?.WaitForInit();

        Sandbox.Engine.Platform.Game.IsDedicated = IsDedicated;
        _game = new(args)
        {
            DrawThread = _renderThread?.RenderThread,
            form = _renderThread?.Surrogate
        };
        
        void OnResize(IEarlyWindow window, EventArgs eventArgs)
        {
            var size = window.ClientSize;
            MySandboxGame.Static.RenderThread_SizeChanged(size.Width, size.Height, new MyViewport(new(size.Width, size.Height)));
        }

        if (_renderThread is not null)
        {
            _renderThread.Window!.Resize += OnResize;
            _renderThread.Surrogate.OnExit += _game.OnExit;
            _renderThread.Surrogate.OnManualWindowCloseRequest += _game.Window_OnManualWindowCloseRequest;
            _renderThread.InitWaiter(_game.m_gameTimer, MyPerGameSettings.MaxFrameRate);
            OnResize(_renderThread.Window, EventArgs.Empty);
        }

        MyRenderProxy.EnableAppEventsCall = false;

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
            _crashPadService?.CaptureCurrentThreadException(e);
            LogManager.GetLogger("Game").Fatal(e, "Fatal exception in game loop");
            return false;
        }

        return true;
    }

    public void Restart()
    {
        RestartRequested = true;
        MySandboxGame.Static.Invoke(CloseGame, nameof(Restart));
    }

    private static void CloseGame()
    {
        MyAudio.Static.Mute = true;
        MyAudio.Static.StopMusic();

        MySessionLoader.Unload();
        MyScreenManager.CloseAllScreensNowExcept(null);
        MySandboxGame.ExitThreadSafe();
    }

    private IServiceProvider SetupServices(ServiceCollection services)
    {
        var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(1));

        services.AddHttpClient<PluginsLifetime, PluginsLifetime>((client, provider) =>
                new PluginsLifetime(provider.GetRequiredService<ConfigHandler>(),
                    provider.GetRequiredService<IPluginServiceProviderFactory>(), client, _dir,
                    IsDedicated ? "CringePluginDedicated" : "CringePlugin"))
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
            .AddSingleton(_ => new ConfigHandler(_configDir))
            .AddSingleton(_crashPadService!);
        
        var factory = new AutofacServiceProviderFactory();

        services.AddSingleton<IServiceProviderFactory<ContainerBuilder>>(factory)
            .AddTransient<IPluginServiceProviderFactory, PluginServiceProviderFactory>();

        return GameServicesExtension.GameServices = factory.CreateServiceProvider(factory.CreateBuilder(services));
    }

    protected virtual async ValueTask<LauncherConfig?> ReadUpdateConfigAsync(Logger logger)
    {
        var path = Path.Join(_configDir.FullName, "launcher.json");

        if (!File.Exists(path))
            return null;

        try
        {
            await using var stream = File.OpenRead(path);

            var conf = await JsonSerializer.DeserializeAsync<LauncherConfig>(stream, ConfigHandler.SerializerOptions);

            return conf;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error reading launcher config");
        }

        return null;
    }

    #region Keen shit

    protected virtual void Initialize(Splash splash)
    {
        splash.DefineStage(new GameServiceInitializationStage(IsDedicated));
    }

    #endregion

    public void Dispose()
    {
        _renderThread?.Dispose();
        _game?.Dispose();
        MyGameService.ShutDown();
        MyInitializer.InvokeAfterRun();
        MyVRage.Done();
    }
}
