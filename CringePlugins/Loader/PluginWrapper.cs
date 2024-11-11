using CringePlugins.Ui;
using ImGuiNET;
using NLog;
using VRage.Plugins;

namespace CringePlugins.Loader;

//todo maybe we could unload the plugin if there's an error?
internal sealed class PluginWrapper(PluginMetadata metadata, IPlugin plugin) : IHandleInputPlugin
{
    public bool HasError => LastException != null;
    public Exception? LastException { get; private set; } //todo: show exception when hovered in plugin menu?

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    
    private readonly IHandleInputPlugin? _handleInputPlugin = plugin as IHandleInputPlugin;

    private const float ErrorShowTime = 10f;

    public void Dispose()
    {
        try
        {
            plugin.Dispose();
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception while Disposing {Metadata}", metadata);
            LastException = e;
            NotificationsComponent.SpawnNotification(ErrorShowTime, RenderError);
        }
    }

    public void HandleInput()
    {
        if (_handleInputPlugin is null || HasError)
            return;

        try
        {
            _handleInputPlugin.HandleInput();
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception while Updating {Metadata}", metadata);
            LastException = e;
            NotificationsComponent.SpawnNotification(ErrorShowTime, RenderError);
        }
    }

    public void Init(object gameInstance)
    {
        try
        {
            plugin.Init(gameInstance);
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception while Initializing {Metadata}", metadata);
            LastException = e;
            NotificationsComponent.SpawnNotification(ErrorShowTime, RenderError);
        }
    }

    public void Update()
    {
        if (HasError)
            return;

        try
        {
            plugin.Update();
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception while Updating {Metadata}", metadata);
            LastException = e;
            NotificationsComponent.SpawnNotification(ErrorShowTime, RenderError);
        }
    }

    public override string ToString() => metadata.ToString();

    private void RenderError()
    {
        ImGui.TextColored(new System.Numerics.Vector4(1f, 0f, 0f, 0f), "Error: ");
        ImGui.SameLine();
        ImGui.TextWrapped($"Fatal error in {metadata.Name}: {LastException?.Message}");
    }
}
