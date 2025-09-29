using CringeLauncher.UserDev.Networking;
using CringePlugins.Config;
using CringePlugins.Splash;
using NLog;
using VRage;
using VRage.GameServices;

namespace CringeLauncher.UserDev;

public class UserDevLauncher() : Launcher(Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR"))
{
    protected override void InitUgc(Splash splash)
    {
#if DEBUG
        Environment.SetEnvironmentVariable("SteamAppId", LauncherConstants.AppId.ToString());
        base.InitUgc(splash);
        return;
#endif
        var gameService = new UserDevGameService(LauncherConstants.AppId);
        MyServiceManager.Instance.AddService<IMyGameService>(gameService);
        MyServiceManager.Instance.AddService<IMyNetworking>(new MyNullNetworking(gameService));
        MyServiceManager.Instance.AddService<IMyLobbyDiscovery>(new MyNullLobbyDiscovery());
        MyServiceManager.Instance.AddService<IMyServerDiscovery>(new MyNullServerDiscovery());
    }

    protected override ValueTask<LauncherConfig?> ReadUpdateConfigAsync(Logger logger)
    {
        return ValueTask.FromResult<LauncherConfig?>(LauncherConfig.Default with { DisableLauncherUpdates = true });
    }
}