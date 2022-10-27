namespace CringePlugins.Splash;

public class Splash : ISplashProgress
{
    private readonly List<ILoadingStage> _loadingStages = [];
    
    public void Report(ProgressInfo value)
    {
    }

    public void Report(float value)
    {
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
        }
    }
}