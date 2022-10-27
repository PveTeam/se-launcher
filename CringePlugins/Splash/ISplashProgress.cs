namespace CringePlugins.Splash;

public interface ISplashProgress : IProgress<ProgressInfo>, IProgress<float> 
{
    void DefineStage(ILoadingStage stage);
    void DefineStepsCount(int count);
}

public interface ILoadingStage
{
    string Name { get; }
    ValueTask Load(ISplashProgress progress);
}