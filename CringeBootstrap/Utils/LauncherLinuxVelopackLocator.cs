using System.Runtime.Versioning;
using Velopack;
using Velopack.Locators;
using Velopack.Logging;
using Velopack.NuGet;

namespace CringeBootstrap.Utils;

[SupportedOSPlatform("linux")]
public class LauncherLinuxVelopackLocator : VelopackLocator
{
    private IVelopackLogger? _logger;

    /// <inheritdoc />
    public override string? AppId { get; }

    /// <inheritdoc />
    public override string? RootAppDir { get; }

    /// <inheritdoc />
    public override string? UpdateExePath { get; }

    /// <inheritdoc />
    public override IProcessImpl Process { get; }

    /// <inheritdoc />
    public override SemanticVersion? CurrentlyInstalledVersion { get; }

    /// <inheritdoc />
    public override string? AppContentDir { get; }

    /// <inheritdoc />
    public override string? Channel { get; }

    /// <inheritdoc />
    public override bool IsPortable => true;

    /// <inheritdoc />
    public override string? AppTempDir => Directory.CreateTempSubdirectory(AppId).FullName;

    /// <inheritdoc />
    public override string? PackagesDir => CreateSubDirIfDoesNotExist(PersistentTempDir, "packages");

    /// <summary> /var/tmp/{velopack}/{appid}, for storing app specific files which need to be preserved. </summary>
    public string? PersistentTempDir => CreateSubDirIfDoesNotExist(PersistentVelopackDir, AppId);

    /// <summary> A pointer to /var/tmp/{velopack}, a location on linux which is semi-persistent. </summary>
    public string? PersistentVelopackDir => CreateSubDirIfDoesNotExist("/var/tmp", "velopack");

    /// <summary> File path of the .AppImage which mounted and ran this application. </summary>
    public string? AppImagePath => Environment.GetEnvironmentVariable("APPIMAGE");
    public string? AppDirPath => Environment.GetEnvironmentVariable("APPDIR");

    public override IVelopackLogger Log => _logger ??= new NullVelopackLogger();

    /// <summary>
    /// Creates a new <see cref="OsxVelopackLocator"/> and auto-detects the
    /// app information from metadata embedded in the .app.
    /// </summary>
    public LauncherLinuxVelopackLocator(IProcessImpl? processImpl, IVelopackLogger? customLog)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException($"Cannot instantiate {nameof(LauncherLinuxVelopackLocator)} on a non-linux system.");
        
        _logger = customLog ?? new NullVelopackLogger();

        Process = processImpl ??= new DefaultProcessImpl(_logger);
        var ourPath = processImpl.GetCurrentProcessPath();

        _logger.Info($"Initializing {nameof(LauncherLinuxVelopackLocator)}");

        // are we inside a mounted .AppImage?
        if (!string.IsNullOrEmpty(AppImagePath) && !string.IsNullOrEmpty(AppDirPath)) {
            var binPath = Path.Join(AppDirPath, "usr", "bin");
            var updateExe = Path.Combine(binPath, "UpdateNix");
            var metadataPath = Path.Combine(binPath, "sq.version");

            if (File.Exists(AppImagePath)) {
                if (File.Exists(updateExe) && PackageManifest.TryParseFromFile(metadataPath, out var manifest)) {
                    _logger.Info("Located valid manifest file at: " + metadataPath);
                    AppId = manifest.Id;
                    RootAppDir = AppImagePath;
                    AppContentDir = binPath;
                    UpdateExePath = updateExe;
                    CurrentlyInstalledVersion = manifest.Version;
                    Channel = manifest.Channel;
                } else {
                    _logger.Error($"Unable to locate UpdateNix in {binPath}");
                }
            } else {
                _logger.Error("Unable to locate .AppImage ($APPIMAGE)");
            }
        } else {
            _logger.Warn(
                $"Unable to locate .AppImage root from '{ourPath}'. This warning indicates that the application is not running from a mounted .AppImage, for example during development.");
        }

        if (AppId == null) {
            _logger.Warn($"Failed to initialise {nameof(LauncherLinuxVelopackLocator)}. This could be because the program is not in an .AppImage.");
        } else {
            _logger.Info($"Initialised {nameof(LauncherLinuxVelopackLocator)} for {AppId} v{CurrentlyInstalledVersion}");
        }
    }
}
