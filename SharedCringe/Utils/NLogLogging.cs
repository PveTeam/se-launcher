using NLog;

namespace SharedCringe.Utils;

internal static class NLogLogging 
{
    public static void Init()
    {
        LogManager.Setup()
            .SetupExtensions(s =>
            {
                s.RegisterLayoutRenderer("cringe-exception", e =>
                {
                    if (e.Exception is null)
                        return string.Empty;
                    e.Exception.FormatStackTrace();
                    return e.Exception.ToString();
                });
            })
            .LoadConfigurationFromFile(optional: false);

        LogManager.ReconfigExistingLoggers();
    }
}