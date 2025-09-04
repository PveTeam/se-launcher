using System.Diagnostics;
using CringeBootstrap.Abstractions;
using CringeLauncher.Render;
using CringeLauncher.Utils;
using CringePlugins.Config;
using CringePlugins.Loader;
using CringePlugins.Render;
using CringePlugins.Services;
using CringePlugins.Splash;
using Epic.OnlineServices.VRage;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Polly;
using Polly.Extensions.Http;
using Sandbox;
using Sandbox.Engine.Networking;
using SpaceEngineers.Game;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
using CringeLauncher.Stages;
using VRage;
using VRage.FileSystem;
using VRageRender;

namespace CringeLauncher;

public class Launcher : ICorePlugin
{
    private readonly string? _gameDataDirectoryPathOverride;
    public const uint AppId = 244850U;
    private SpaceEngineersGame? _game;
    private IPluginsLifetime? _lifetime;

    private readonly DirectoryInfo _configDir;
    private readonly DirectoryInfo _dir;
    private EarlyRenderThread? _renderThread;

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
                    if (e.Exception is null)
                        return string.Empty;
                    e.Exception.FormatStackTrace();
                    return e.Exception.ToString();
                });
            })
            .LoadConfigurationFromFile(optional: false);

        LogManager.ReconfigExistingLoggers();

        var logger = LogManager.GetLogger("CringeBootstrap");
        logger.Info("Bootstrapping");

        var serviceProvider = SetupServices();

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ImGuiHandler.Instance = new(_configDir);

        var keepConsole = Debugger.IsAttached || args.Contains("-keepconsole", StringComparer.OrdinalIgnoreCase);
        _renderThread = new EarlyRenderThread(keepConsole);
        
        using var splash = new Splash();
        RenderHandler.Current.RegisterComponent(splash);
        
        splash.DefineStage(new CheckUpdatesStage(args, ReadUpdateConfigAsync));
        splash.DefineStage(new LauncherPatchesStage());

        //environment variable for viktor's plugins
        Environment.SetEnvironmentVariable("SE_PLUGIN_DISABLE_METHOD_VERIFICATION", "True");

        // hook up steam as we ship it inside base context as an override
        if (AssemblyLoadContext.GetLoadContext(typeof(Launcher).Assembly) is ICoreLoadContext coreLoadContext)
            NativeLibrary.SetDllImportResolver(typeof(Steamworks.Constants).Assembly, (name, _, _) => coreLoadContext.ResolveUnmanagedDll(name));
        NativeLibrary.SetDllImportResolver(typeof(EosService).Assembly, (name, _, _) => NativeLibrary.Load(Path.Join(AppContext.BaseDirectory, name)));
        
        splash.ExecuteLoadingStages();

        MyFileSystem.ExePath = Path.GetDirectoryName(args.ElementAtOrDefault(0) ?? Assembly.GetExecutingAssembly().Location)!;
        MyFileSystem.RootPath = new DirectoryInfo(MyFileSystem.ExePath).Parent!.FullName;
        
        splash.DefineStage(new PlatformInitializationStage(_renderThread, _gameDataDirectoryPathOverride));
        splash.DefineStage(new RenderInitializationStage(_renderThread));
        splash.DefineStage(_lifetime = serviceProvider.GetRequiredService<IPluginsLifetime>());

        InitUgc(splash);
        
        // this technically should wait for render thread init, but who cares
        splash.ExecuteLoadingStages();
        
        MyFileSystem.InitUserSpecific(MyGameService.UserId.ToString());

        _lifetime.RegisterLifetime();

        _renderThread.WaitForInit();

        _game = new(args)
        {
            DrawThread = _renderThread.RenderThread,
            form = _renderThread.Surrogate
        };
        
        void OnResize(object? o, EventArgs eventArgs)
        {
            var size = _renderThread.Window.ClientSize;
            MySandboxGame.Static.RenderThread_SizeChanged(size.Width, size.Height, new MyViewport(new(size.Width, size.Height)));
        }

        _renderThread.Window!.Resize += OnResize;
        _renderThread.Surrogate.OnExit += _game.OnExit;
        _renderThread.Surrogate.OnManualWindowCloseRequest += _game.Window_OnManualWindowCloseRequest;

        MyRenderProxy.EnableAppEventsCall = false;

        OnResize(null, EventArgs.Empty);

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

    protected virtual void InitUgc(Splash splash)
    {
        splash.DefineStage(new GameServiceInitializationStage());
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