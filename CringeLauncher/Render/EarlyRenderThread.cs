using Windows.Win32;
using VRage;
using VRageRender;

namespace CringeLauncher.Render;

internal class EarlyRenderThread
{
    private readonly bool _keepConsole;
    public EarlyWindow? Window { get; private set; }
    private readonly ManualResetEventSlim _initEvent = new(false);
    public Thread RenderThread { get; }
    private bool _gameRendererInitialized;

    public VRageWindowSurrogate Surrogate =>
        Window?.Surrogate ?? throw new InvalidOperationException("Call WaitForInit until the render thread is initialized");

    public EarlyRenderThread(bool keepConsole)
    {
        _keepConsole = keepConsole;
        RenderThread = new Thread(RunLoop)
        {
            Name = "Early Render Thread"
        };

        RenderThread.SetApartmentState(ApartmentState.STA);
        RenderThread.Start();
    }
    
    public void NotifyGameRendererInitialized() => _gameRendererInitialized = true;

    public void WaitForInit() => _initEvent.Wait();

    private void RunLoop()
    {
        Window = new() { Text = "Cringe Launcher" };
        Window.Show();
        Window.Activate();
        _initEvent.Set();
        if (!_keepConsole)
        {
            Console.SetOut(new StreamWriter(Stream.Null));
            Console.SetError(new StreamWriter(Stream.Null));
            Console.SetIn(new StreamReader(Stream.Null));
            PInvoke.FreeConsole();
        }
        while (true)
        {
            if (!_gameRendererInitialized)
            {
                Window.Frame();
                continue;
            }
            
            if (!Surrogate.UpdateRenderThread()) break;
            RenderFrame();
        }
        
        if (_gameRendererInitialized) DisposeGameRenderer();
    }

    private void DisposeGameRenderer()
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

        MyRenderProxy.BeforeRender(null);
        MyRenderProxy.Draw();
        MyRenderProxy.AfterRender();
        
        Window!.Draw();
        
        MyRenderProxy.Present();
    }
}