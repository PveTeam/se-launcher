using NLog;
using Pillar.Demystifier;

namespace CringeLauncher.CrashPad;

internal class NLogLoggerWrapper(Logger logger) : IStackTraceExceptionLogger
{
    public void LogException(Exception exception, string? message)
    {
        if (message is null)
            logger.Error(exception);
        else
            logger.Error(exception, message);
    }
}