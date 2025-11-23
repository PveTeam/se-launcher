using Sandbox;

namespace CringePlugins.Services;

public static class GameServicesExtension
{
    internal static IServiceProvider GameServices { get; set; } = null!;
    /* 
        extension(MySandboxGame)
        {
            public static IServiceProvider Services => GameServices;
        } */
}