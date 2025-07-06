using CringeLauncher.UserDev.Networking;
using NLog;
using VRage;
using VRage.GameServices;

namespace CringeLauncher.UserDev;

public class UserDevLauncher() : Launcher(Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR"))
{
    protected override void InitUgc()
    {
        var gameService = new UserDevGameService(AppId);
        MyServiceManager.Instance.AddService<IMyGameService>(gameService);
        MyServiceManager.Instance.AddService<IMyNetworking>(new MyNullNetworking(gameService));
        MyServiceManager.Instance.AddService<IMyLobbyDiscovery>(new MyNullLobbyDiscovery());
        MyServiceManager.Instance.AddService<IMyServerDiscovery>(new MyNullServerDiscovery());
    }

    protected override Task<bool> CheckUpdatesDisabledAsync(Logger logger)
    {
        return Task.FromResult(true);
    }
}