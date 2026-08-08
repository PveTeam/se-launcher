using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CliWrap;
using CliWrap.Buffered;
using NLog;
using Polly;

namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>
/// Reads crash metadata and the journal-provided native backtrace via coredumpctl.
/// Linux-only. All permission / missing-binary / no-match failures are soft (return null).
/// </summary>
internal sealed class SystemdCoredumpCrashSource : ICrashSupplementalSource
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    // Journal entry can lag process exit; sleep sequence matches prior manual delays after first try.
    private static readonly TimeSpan[] RetrySleeps =
    [
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(600),
        TimeSpan.FromMilliseconds(1200)
    ];

    private static readonly Policy<CoredumpctlInfo?> QueryRetryPolicy = Policy
        .HandleResult<CoredumpctlInfo?>(info => info is null)
        .WaitAndRetry(
            RetrySleeps,
            onRetry: static (_, delay, attempt, _) =>
                Log.Debug("coredumpctl miss; retry {Attempt} after {DelayMs}ms", attempt, delay.TotalMilliseconds));

    public string Id => "systemd-coredump";

    public CrashSupplementalSection? TryCollect(CrashSupplementalContext context)
    {
        if (!OperatingSystem.IsLinux())
            return null;

        if (!IsCoredumpctlAvailable())
        {
            Log.Debug("coredumpctl not available; skipping systemd coredump source");
            return null;
        }

        var info = QueryRetryPolicy.Execute(() => TryQueryInfo(context));

        if (info is null)
            return null;

        var body = Format(info);
        if (string.IsNullOrWhiteSpace(body))
            return null;

        var signal = info.SignalName ?? (info.Signal is { } sig ? $"signal {sig}" : "signal unknown");
        var summary = info.Executable is { Length: > 0 } exe
            ? $"systemd coredump: {signal} in {exe}"
            : $"systemd coredump: {signal}";

        return new CrashSupplementalSection
        {
            SourceId = Id,
            Title = "Native Crash (systemd-coredump)",
            Body = body,
            Priority = 80,
            Summary = summary
        };
    }

    private static bool IsCoredumpctlAvailable()
    {
        try
        {
            var path = FindOnPath("coredumpctl");
            return path is not null;
        }
        catch (Exception e)
        {
            Log.Debug(e, "Failed to resolve coredumpctl");
            return false;
        }
    }

    private static string? FindOnPath(string fileName)
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return null;

        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Join(dir, fileName);
            if (File.Exists(candidate))
                return candidate;
        }

        // Common absolute locations when PATH is minimal (e.g. stripped desktop launchers).
        foreach (var candidate in new[] { "/usr/bin/coredumpctl", "/bin/coredumpctl" })
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private CoredumpctlInfo? TryQueryInfo(CrashSupplementalContext context)
    {
        // Prefer exact PID match; fall back to executable path filter.
        var info = TryRunInfoJson(context.ProcessId.ToString(CultureInfo.InvariantCulture));
        if (info?.Pid is { } pid && pid != context.ProcessId)
        {
            Log.Debug("coredumpctl returned pid {Actual} while expecting {Expected}", pid, context.ProcessId);
            info = null;
        }

        if (info is not null || string.IsNullOrEmpty(context.ExecutablePath)) return info;
        
        info = TryRunInfoJson(context.ExecutablePath);
        if (info?.Pid is not { } exePid || exePid == context.ProcessId) return info;
        
        // Executable fallback can hit an older dump of the same binary — reject pid mismatch.
        Log.Debug("Ignoring coredump for {Exe} pid {Actual}, expected {Expected}",
            context.ExecutablePath, exePid, context.ProcessId);
        info = null;

        return info;
    }

    private static CoredumpctlInfo? TryRunInfoJson(string match)
    {
        try
        {
            var (exitCode, stdout, stderr) = Cli.Wrap("coredumpctl")
                .WithArguments(["info", match, "--json=pretty", "--quiet"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            if (exitCode != 0)
            {
                // Expected when no dump exists or journal is inaccessible — do not warn loudly.
                if (!string.IsNullOrWhiteSpace(stderr))
                    Log.Debug("coredumpctl info {Match} exited {Code}: {Stderr}", match, exitCode,
                        TrimForLog(stderr));
                else
                    Log.Debug("coredumpctl info {Match} exited {Code} (no match or no permission)", match,
                        exitCode);
                return null;
            }

            if (string.IsNullOrWhiteSpace(stdout))
                return null;

            return DeserializeInfo(stdout);
        }
        catch (Exception e) when (e is FileNotFoundException or UnauthorizedAccessException or TimeoutException
                                      or System.ComponentModel.Win32Exception)
        {
            Log.Debug(e, "coredumpctl info invocation failed for {Match}", match);
            return null;
        }
        catch (Exception e)
        {
            // Includes DBus/journal permission failures surfaced as generic exceptions.
            Log.Debug(e, "coredumpctl info failed for {Match}", match);
            return null;
        }
    }

    private static CoredumpctlInfo? DeserializeInfo(string stdout)
    {
        try
        {
            var trimmed = stdout.TrimStart();
            if (trimmed.StartsWith('['))
            {
                var list = JsonSerializer.Deserialize<List<CoredumpctlInfo>>(stdout, JsonOptions);
                return list is { Count: > 0 } ? list[0] : null;
            }

            return JsonSerializer.Deserialize<CoredumpctlInfo>(stdout, JsonOptions);
        }
        catch (JsonException e)
        {
            Log.Debug(e, "Failed to parse coredumpctl JSON");
            return null;
        }
    }

    private static string Format(CoredumpctlInfo info)
    {
        var sb = new StringBuilder();

        if (info.Pid is { } pid)
            sb.Append("PID: ").AppendLine(pid.ToString(CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(info.Executable))
            sb.Append("Executable: ").AppendLine(info.Executable);
        if (!string.IsNullOrEmpty(info.CommandLine))
            sb.Append("CommandLine: ").AppendLine(info.CommandLine);
        else if (!string.IsNullOrEmpty(info.Command))
            sb.Append("Command: ").AppendLine(info.Command);

        if (!string.IsNullOrEmpty(info.SignalName) || info.Signal is not null)
        {
            sb.Append("Signal: ");
            if (!string.IsNullOrEmpty(info.SignalName))
                sb.Append(info.SignalName);
            if (info.Signal is { } sig)
            {
                if (!string.IsNullOrEmpty(info.SignalName))
                    sb.Append(" (").Append(sig).Append(')');
                else
                    sb.Append(sig);
            }

            sb.AppendLine();
        }

        if (info.Timestamp is { } ts)
        {
            // coredumpctl JSON uses microseconds since epoch.
            try
            {
                var dto = DateTimeOffset.FromUnixTimeMilliseconds(ts / 1000);
                sb.Append("Timestamp: ").AppendLine(dto.ToString("u", CultureInfo.InvariantCulture));
            }
            catch (ArgumentOutOfRangeException)
            {
                sb.Append("Timestamp(raw): ").AppendLine(ts.ToString(CultureInfo.InvariantCulture));
            }
        }

        if (!string.IsNullOrEmpty(info.Storage))
            sb.Append("Storage: ").AppendLine(info.Storage);
        if (!string.IsNullOrEmpty(info.Filename))
            sb.Append("Core file: ").AppendLine(info.Filename);
        if (info.DiskSize is { } size)
            sb.Append("Core size: ").Append(size.ToString(CultureInfo.InvariantCulture)).AppendLine(" bytes");
        if (!string.IsNullOrEmpty(info.Hostname))
            sb.Append("Hostname: ").AppendLine(info.Hostname);
        if (!string.IsNullOrEmpty(info.Unit))
            sb.Append("Unit: ").AppendLine(info.Unit);
        if (!string.IsNullOrEmpty(info.UserUnit))
            sb.Append("UserUnit: ").AppendLine(info.UserUnit);

        if (!string.IsNullOrWhiteSpace(info.Message))
        {
            sb.AppendLine();
            sb.AppendLine("-- Journal message / backtrace --");
            sb.AppendLine(info.Message.TrimEnd());
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("No journal backtrace message available (core metadata only).");
        }

        return sb.ToString();
    }

    private static string TrimForLog(string text)
    {
        var oneLine = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
        return oneLine.Length <= 300 ? oneLine : oneLine[..300] + "…";
    }

    private sealed class CoredumpctlInfo
    {
        public int? Pid { get; set; }
        public int? Uid { get; set; }
        public int? Gid { get; set; }
        public int? Signal { get; set; }
        public string? SignalName { get; set; }
        public long? Timestamp { get; set; }
        public string? Executable { get; set; }
        public string? Command { get; set; }
        public string? CommandLine { get; set; }
        public string? Storage { get; set; }
        public string? Filename { get; set; }
        public long? DiskSize { get; set; }
        public string? Hostname { get; set; }
        public string? Unit { get; set; }
        public string? UserUnit { get; set; }
        public string? Message { get; set; }
    }
}
