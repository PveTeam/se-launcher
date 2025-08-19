using CringePlugins.Splash;
using Epic.OnlineServices.VRage;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Sandbox.Game;
using SpaceEngineers.Game.Achievements;
using VRage;
using VRage.GameServices;
using VRage.Mod.Io;
using VRage.Steam;

namespace CringeLauncher.Stages;

public class GameServiceInitializationStage() : ILoadingStage
{
    public string Name { get; } = "Game service initialization";
    public ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(2);
        
        progress.Report("Steam game service initialization");
        
        var steamGameService = MySteamGameService.Create(false, Launcher.AppId);
        MyServiceManager.Instance.AddService(steamGameService);

        var aggregator = new MyServerDiscoveryAggregator();
        MySteamGameService.InitNetworking(false, steamGameService, MyPerGameSettings.GameName, aggregator);
        
        progress.Report("Epic game service initialization");
        
        EosService.InitNetworking(false, false, MyPerGameSettings.GameName, steamGameService, "xyza7891964JhtVD93nm3nZp8t1MbnhC",
            "AKGM16qoFtct0IIIA8RCqEIYG4d4gXPPDNpzGuvlhLA", "24b1cd652a18461fa9b3d533ac8d6b5b",
            "1958fe26c66d4151a327ec162e4d49c8", "07c169b3b641401496d352cad1c905d6",
            "https://retail.epicgames.com/", EosService.CreatePlatform(),
            MyPlatformGameSettings.VERBOSE_NETWORK_LOGGING, [], aggregator,
            MyMultiplayer.Channels);

        MyServiceManager.Instance.AddService<IMyServerDiscovery>(aggregator);

        MyServiceManager.Instance.AddService(MySteamGameService.CreateMicrophone());

        MyGameService.WorkshopService.AddAggregate(MySteamUgcService.Create(Launcher.AppId, steamGameService));

        var modUgc = MyModIoService.Create(MyServiceManager.Instance.GetService<IMyGameService>(), "spaceengineers",
            "264",
            "1fb4489996a5e8ffc6ec1135f9985b5b", "331", "f2b64abe55452252b030c48adc0c1f0e",
            MyPlatformGameSettings.UGC_TEST_ENVIRONMENT, false, MyPlatformGameSettings.MODIO_PLATFORM,
            MyPlatformGameSettings.MODIO_PORTAL);
        modUgc.IsConsentGiven = MySandboxGame.Config.ModIoConsent;
        MyGameService.WorkshopService.AddAggregate(modUgc);

        MySpaceEngineersAchievements.Initialize();
        
        return default;
    }
}