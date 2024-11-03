using NLog;

namespace CringePlugins.Splash;

public class Splash : ISplashProgress
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly List<ILoadingStage> _loadingStages = [];

    private ProgressInfo? _lastInfo;
    
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
        foreach (var loadingStage in _loadingStages)
        {
            // todo sync context
            loadingStage.Load(this).AsTask().GetAwaiter().GetResult();
            _lastInfo = null;
        }
    }
}