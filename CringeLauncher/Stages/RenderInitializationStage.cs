using CringeLauncher.Render;
using CringePlugins.Splash;
using Sandbox;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Game;
using SpaceEngineers.Game.GUI;
using VRage;
using VRage.Platform.Windows.Render;
using VRage.UserInterface;
using VRage.Utils;
using VRageRender;

namespace CringeLauncher.Stages;

internal class RenderInitializationStage(EarlyRenderThread renderThread) : ILoadingStage
{
    public string Name { get; } = "Render initialization";
    public ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(1);
        
        progress.Report("Initializing render");

        renderThread.Window!.Invoke(InitRender);
        
        return default;
    }
    
    private void InitRender()
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
        
        MyPlatformRender.Log = MyLog.Default;
        var settings = MyRenderProxy.CreateDevice(null, MyVideoSettingsManager.Initialize(), out _);
        MyRenderProxy.SendCreatedDeviceSettings(settings);
        
        renderThread.NotifyGameRendererInitialized();
        ImGuiHandler.Instance?.NotifyGameRendererInitialized();
    }
}