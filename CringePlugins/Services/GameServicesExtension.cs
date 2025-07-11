using Sandbox;

namespace CringePlugins.Services;

public static class GameServicesExtension
{
    public static IServiceProvider GameServices { get; set; } = null!;

    extension(MySandboxGame)
    {
        public static IServiceProvider Services => GameServices;
    }
}