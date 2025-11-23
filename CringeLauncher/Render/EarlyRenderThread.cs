using CringeLauncher.Utils;
using Sandbox.Engine.Utils;
using VRage;
using VRage.Library.Utils;
using VRageRender;

namespace CringeLauncher.Render;

internal class EarlyRenderThread : IDisposable
{
    private readonly bool _keepConsole;
    public IEarlyWindow? Window { get; private set; }
    private readonly ManualResetEventSlim _initEvent = new(false);

    private MyGameTimer? _timer;
    private WaitForTargetFrameRate? _waiter;

    public Thread? RenderThread { get; private set; }
    private bool _gameRendererInitialized;

    public VRageWindowSurrogate Surrogate =>
        Window?.Surrogate ?? throw new InvalidOperationException("Call WaitForInit until the render thread is initialized");

    public EarlyRenderThread(bool keepConsole)
    {
        _keepConsole = keepConsole;
        const string threadName = "Early Render Thread";
#if WINDOWS
        RenderThread = new Thread(RunLoop)
        {
            Name = threadName
        };
        
        RenderThread.SetApartmentState(ApartmentState.STA);
        RenderThread.Start();
#else
        PlatformApi.CreateThread(RunLoop, threadName);
#endif
    }

    public void NotifyGameRendererInitialized() => _gameRendererInitialized = true;

    public void WaitForInit() => _initEvent.Wait();

    public void InitWaiter(MyGameTimer timer, float targetFrameRate)
    {
        if (targetFrameRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetFrameRate), "Must be positive");

        _timer = timer;
        _waiter = new WaitForTargetFrameRate(_timer, targetFrameRate);
    }

    public void SetTargetFrameRate(float targetFrameRate)
    {
        if (targetFrameRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetFrameRate), "Must be positive");

        if (_timer is null)
            throw new InvalidOperationException("Call InitWaiter before setting target frame rate");

        _waiter = new WaitForTargetFrameRate(_timer, targetFrameRate);
    }

    private void RunLoop()
    {
        RenderThread = Thread.CurrentThread;
#if WINDOWS
        Window = new Win.EarlyWindow
#else
        Window = new Xplat.EarlyWindow
#endif
            { Title = "Cringe Launcher" };
        
        Window.Activate();
        
        _initEvent.Set();
        
        if (!_keepConsole) ConsoleHandler.FreeConsole();
        
        while (true)
        {
            if (!_gameRendererInitialized)
            {
                if (!Window.Frame()) break;
                continue;
            }

            if (!Surrogate.UpdateRenderThread()) break;
            RenderFrame();
        }

        if (_gameRendererInitialized) DisposeGameRenderer();
    }

    private static void DisposeGameRenderer()
    {
        MyRenderProxy.AfterUpdate(null);
        MyRenderProxy.BeforeUpdate();
        MyRenderProxy.UnloadContent();
        MyRenderProxy.ProcessMessages();
        MyRenderProxy.DisposeDevice();
    }

    private void RenderFrame()
    {
        if (MyVRage.Platform.Ansel.IsCaptureRunning)
        {
            MyRenderProxy.Ansel_DrawScene();
            MyRenderProxy.Present();
            return;
        }

        _waiter?.Wait();

        MyRenderProxy.BeforeRender(null);
        MyFpsManager.Update();
        MyRenderProxy.Draw();
        MyRenderProxy.AfterRender();

        Window!.Draw();

        MyRenderProxy.Present();
    }

    public void Dispose()
    {
        // if is null or the game renderer has not yet taken ownership of the swap chain
        if (Window is null or { OwnsSwapChain: true }) return;
        Surrogate.Exit();
    }
}
