using System.Numerics;
using CringePlugins.Abstractions;
using CringePlugins.Services;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using NLog;

using static ImGuiNET.ImGui;

namespace CringePlugins.Splash;

public class Splash : ISplashProgress, IRenderComponent
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly List<ILoadingStage> _loadingStages = [];

    private ProgressInfo? _lastInfo;
    private bool _done;
    private readonly string _splashPath = Path.Join(AppContext.BaseDirectory, "splash.png");

    private readonly IImGuiImageService _imageService =
        GameServicesExtension.GameServices.GetRequiredService<IImGuiImageService>();
    
    public void Report(ProgressInfo value)
    {
        _lastInfo = value;

        if (value is PercentProgressInfo percentProgressInfo)
            Logger.Info("{Text} {Percent:P0}", percentProgressInfo.Text, percentProgressInfo.Percent);
        else
            Logger.Info("{Text}", value.Text);
    }

    public void Report(float value)
    {
        if (_lastInfo is not null)
            Logger.Info("{Text} {Percent:P0}", _lastInfo.Text, value);
    }

    public void DefineStage(ILoadingStage stage) => _loadingStages.Add(stage);

    public void DefineStepsCount(int count)
    {
    }

    public void ExecuteLoadingStages()
    {
        try
        {
            foreach (var loadingStage in _loadingStages)
            {
                // todo sync context
                loadingStage.Load(this).AsTask().GetAwaiter().GetResult();
                _lastInfo = null;
            }
        }
        finally
        {
            _done = true;
        }
    }

    public void OnFrame()
    {
        if (_done) return;

        SetNextWindowPos(GetMainViewport().GetCenter(), ImGuiCond.Always, new(.5f, .5f));
        const int imageSize = 512;
        SetNextWindowSize(new(512, GetFrameHeightWithSpacing() * 2 + imageSize), ImGuiCond.Always);
        Begin("Splash", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs);

        var image = _imageService.GetFromPath(_splashPath);
        Image(image, new(imageSize));

        var sizeArg = new Vector2(GetWindowWidth() - GetStyle().WindowPadding.X * 2, 0);
        if (_lastInfo is null)
        {
            const string text = "Loading...";
            var size = CalcTextSize(text);

            SetCursorPosX((GetWindowWidth() - size.X) * .5f);
            Text(text);
        }
        else
            ProgressBar((_lastInfo as PercentProgressInfo)?.Percent ?? 0, sizeArg, _lastInfo.Text);

        End();
    }
}