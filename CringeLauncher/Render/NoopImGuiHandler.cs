using System.Runtime.Versioning;

namespace CringeLauncher.Render;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
internal sealed class NoopImGuiHandler(DirectoryInfo configDir) : ImGuiHandler(configDir)
{
    public override bool BlockKeys => false;
}
