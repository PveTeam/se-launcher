using NLog;
using Velopack.Logging;

namespace CringeBootstrap.Utils;

internal class VelopackNLogLogger : IVelopackLogger
{
    private readonly Logger _log = LogManager.GetLogger("Velopack");
    public void Log(VelopackLogLevel logLevel, string? message, Exception? exception)
    {
        _log.Log(logLevel switch
        {
            VelopackLogLevel.Trace => LogLevel.Trace,
            VelopackLogLevel.Debug => LogLevel.Debug,
            VelopackLogLevel.Information => LogLevel.Info,
            VelopackLogLevel.Warning => LogLevel.Warn,
            VelopackLogLevel.Error => LogLevel.Error,
            VelopackLogLevel.Critical => LogLevel.Fatal,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        }, exception, 
#pragma warning disable CA2254
            message ?? string.Empty);
#pragma warning restore CA2254
    }
}
