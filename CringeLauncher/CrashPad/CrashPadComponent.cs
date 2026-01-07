using System.Text;
using CringePlugins.Abstractions;
using ImGuiNET;
using NLog;
using static ImGuiNET.ImGui;

namespace CringeLauncher.CrashPad;

internal class CrashPadComponent : IRenderComponent
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static readonly Uri UploadUri = new("https://hastebin.skyra.pw/");
    private readonly HastebinUploader _uploader = new(UploadUri);
    private bool _visible = true;
    private readonly ManualResetEventSlim _exitEvent;
    private readonly string _crashReport;

    private bool _uploading;
    private bool _failed;
    private string? _uploadedUrl;

    public CrashPadComponent(CrashInformation? information, CrashProcessInformation processInformation,
        ManualResetEventSlim exitEvent)
    {
        _exitEvent = exitEvent;

        var memStream = new MemoryStream();
        new CrashReportWriter(information ?? new()
        {
            Network = new(),
            Plugins = [],
            ModScripts = [],
            Version = new()
        }, processInformation).Write(memStream);

        _crashReport = Encoding.UTF8.GetString(memStream.ToArray());
    }

    public void OnFrame()
    {
        if (!_visible) return;
        
        SetNextWindowPos(GetMainViewport().GetCenter(), ImGuiCond.Appearing, new(.5f, .5f));
        SetNextWindowSize(new(700, GetMainViewport().Size.Y / 2), ImGuiCond.FirstUseEver);
        if (Begin("CrashPad Dialog", ref _visible))
        {
            if (Button("Copy to clipboard"))
                SetClipboardText(_crashReport);
            SameLine();

            if (_uploading || _failed)
            {
                BeginDisabled();
                Button(_uploading ? "Uploading..." : "Failed to upload");
                EndDisabled();
            }
            else
            {
                if (Button(string.IsNullOrEmpty(_uploadedUrl) ? "Upload" : "Copy uploaded URL"))
                    UploadAsync();
            }

            if (IsItemHovered(ImGuiHoveredFlags.ForTooltip))
            {
                SetTooltip("Upload to Hastebin\n" +
                           "Warning: This is community-run service, use at your own risk!\n\n" +
                           $"Current host: {UploadUri.Host}");
            }
            
            TextUnformatted(_crashReport);
        }

        End();

        if (!_visible) _exitEvent.Set();
    }

    private async void UploadAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_uploadedUrl))
            {
                _uploading = true;
                _uploadedUrl = await _uploader.UploadAsync(_crashReport);
            }
            
            // dispatcher context should be set from window thread
            SetClipboardText(_uploadedUrl);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to upload crash report");
            _failed = true;
        }
        finally
        {
            _uploading = false;
        }
    }

}