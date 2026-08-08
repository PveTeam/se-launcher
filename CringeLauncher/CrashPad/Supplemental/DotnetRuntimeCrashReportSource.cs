using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;

namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>
/// Reads the JSON crash report written by .NET createdump when DOTNET_EnableCrashReport=1.
/// Enabled for this product on Linux only (runtime can produce it on Unix broadly).
/// File name: "{dumpPath}.crashreport.json".
/// </summary>
internal sealed class DotnetRuntimeCrashReportSource : ICrashSupplementalSource
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters =
        {
            new FlexibleBoolConverter(),
            new FlexibleUInt64Converter(),
            new FlexibleInt32Converter(),
            new FlexibleUInt32Converter()
        }
    };

    public string Id => "dotnet-crashreport";

    public CrashSupplementalSection? TryCollect(CrashSupplementalContext context)
    {
        if (!OperatingSystem.IsLinux())
            return null;
        var path = ResolveReportPath(context.DumpPath);
        if (path is null)
            return null;

        try
        {
            using var stream = File.OpenRead(path);
            var report = JsonSerializer.Deserialize<RuntimeCrashReportDocument>(stream, JsonOptions);
            if (report?.Payload is null)
            {
                Log.Debug("Runtime crash report at {Path} had no payload", path);
                return null;
            }

            var body = Format(report, path);
            if (string.IsNullOrWhiteSpace(body))
                return null;

            var crashed = report.Payload.Threads?.FirstOrDefault(t => t.Crashed);
            var summary = BuildSummary(report, crashed);

            return new CrashSupplementalSection
            {
                SourceId = Id,
                Title = "Runtime Crash Report (.NET)",
                Body = body,
                Priority = 100,
                Summary = summary
            };
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to read runtime crash report from {Path}", path);
            return null;
        }
    }

    private static string? ResolveReportPath(string dumpPath)
    {
        // createdump writes: dumpPath + ".crashreport.json"
        var primary = dumpPath + ".crashreport.json";
        if (File.Exists(primary))
            return primary;

        // Tolerate alternate naming if dump extension was replaced rather than appended.
        var alt = Path.ChangeExtension(dumpPath, ".crashreport.json");
        if (!string.IsNullOrEmpty(alt) && !string.Equals(alt, primary, StringComparison.Ordinal) && File.Exists(alt))
            return alt;

        return null;
    }

    private static string? BuildSummary(RuntimeCrashReportDocument report, RuntimeCrashThread? crashed)
    {
        var exceptionType = report.Parameters?.ExceptionType;
        var managedType = crashed?.ManagedExceptionType;
        var processName = report.Payload?.ProcessName;

        if (!string.IsNullOrEmpty(managedType))
            return string.IsNullOrEmpty(processName)
                ? $"Runtime crash: {managedType}"
                : $"Runtime crash in {processName}: {managedType}";

        if (!string.IsNullOrEmpty(exceptionType))
            return string.IsNullOrEmpty(processName)
                ? $"Runtime crash: {DescribeExceptionType(exceptionType)}"
                : $"Runtime crash in {processName}: {DescribeExceptionType(exceptionType)}";

        return string.IsNullOrEmpty(processName) ? "Runtime crash report available" : $"Runtime crash in {processName}";
    }

    private static string Format(RuntimeCrashReportDocument report, string path)
    {
        var sb = new StringBuilder();
        sb.Append("Source file: ").AppendLine(path);

        var payload = report.Payload!;
        if (!string.IsNullOrEmpty(payload.ProcessName))
            sb.Append("Process: ").AppendLine(payload.ProcessName);

        if (payload.Configuration is { } config)
        {
            if (!string.IsNullOrEmpty(config.Architecture))
                sb.Append("Architecture: ").AppendLine(config.Architecture);
            if (!string.IsNullOrEmpty(config.Version))
                sb.Append("Runtime: ").AppendLine(config.Version.Trim());
        }

        if (!string.IsNullOrEmpty(payload.ProtocolVersion))
            sb.Append("Protocol: ").AppendLine(payload.ProtocolVersion);

        if (report.Parameters?.ExceptionType is { Length: > 0 } exceptionType)
            sb.Append("ExceptionType: ").Append(exceptionType)
                .Append(" (").Append(DescribeExceptionType(exceptionType)).Append(')').AppendLine();

        var threads = payload.Threads;
        if (threads is null || threads.Count == 0)
        {
            sb.AppendLine("No thread information in crash report.");
            return sb.ToString();
        }

        // Crashed thread first, then the rest (cap to keep the dialog usable).
        const int maxThreads = 8;
        const int maxFrames = 32;
        var ordered = threads
            .Select((t, i) => (Thread: t, Index: i))
            .OrderByDescending(t => t.Thread.Crashed)
            .ThenBy(t => t.Index)
            .Take(maxThreads)
            .ToArray();

        sb.Append("Threads: ").Append(threads.Count);
        if (threads.Count > maxThreads)
            sb.Append(" (showing ").Append(maxThreads).Append(')');
        sb.AppendLine();

        foreach (var (thread, index) in ordered)
        {
            sb.AppendLine();
            sb.Append("-- Thread #").Append(index);
            if (thread.Crashed)
                sb.Append(" [CRASHED]");
            sb.Append(" (native_id=").Append(FormatHex(thread.NativeThreadId)).Append(')');
            sb.Append(thread.IsManaged ? " managed" : " native");
            sb.AppendLine(" --");

            if (!string.IsNullOrEmpty(thread.ManagedExceptionType))
                sb.Append("Managed exception: ").AppendLine(thread.ManagedExceptionType);
            if (thread.ManagedExceptionHResult is not null and not 0)
                sb.Append("HRESULT: 0x").AppendLine(thread.ManagedExceptionHResult.Value.ToString("x8", CultureInfo.InvariantCulture));
            if (thread.ManagedExceptionObject is not null and not 0)
                sb.Append("Exception object: ").AppendLine(FormatHex(thread.ManagedExceptionObject.Value));

            if (thread.Ctx is { } ctx)
            {
                sb.Append("IP=").Append(FormatHex(ctx.Ip));
                sb.Append(" SP=").Append(FormatHex(ctx.Sp));
                sb.Append(" BP=").AppendLine(FormatHex(ctx.Bp));
            }

            var frames = thread.StackFrames;
            if (frames is null || frames.Count == 0)
            {
                sb.AppendLine("  (no stack frames)");
                continue;
            }

            var frameCount = Math.Min(frames.Count, maxFrames);
            for (var f = 0; f < frameCount; f++)
                AppendFrame(sb, f, frames[f]);

            if (frames.Count > maxFrames)
                sb.Append("  … ").Append(frames.Count - maxFrames).AppendLine(" more frames");
        }

        if (threads.Count > maxThreads)
            sb.AppendLine().Append("… ").Append(threads.Count - maxThreads).AppendLine(" additional threads omitted");

        return sb.ToString();
    }

    private static void AppendFrame(StringBuilder sb, int index, RuntimeCrashStackFrame frame)
    {
        if (frame.Repeated is int repeated && repeated > 0)
        {
            sb.Append("  #").Append(index).Append(" repeated x").Append(repeated).AppendLine();
            if (frame.RepeatedFrames is { Count: > 0 } repeatedFrames)
            {
                var limit = Math.Min(repeatedFrames.Count, 8);
                for (var i = 0; i < limit; i++)
                    AppendFrameLine(sb, index, repeatedFrames[i], nested: true);
            }

            return;
        }

        AppendFrameLine(sb, index, frame, nested: false);
    }

    private static void AppendFrameLine(StringBuilder sb, int index, RuntimeCrashStackFrame frame, bool nested)
    {
        sb.Append(nested ? "      " : "  #").Append(nested ? "·" : index.ToString(CultureInfo.InvariantCulture));
        sb.Append(frame.IsManaged ? " [managed] " : " [native]  ");

        if (!string.IsNullOrEmpty(frame.MethodName))
            sb.Append(frame.MethodName);
        else if (!string.IsNullOrEmpty(frame.UnmanagedName))
            sb.Append(frame.UnmanagedName);
        else
            sb.Append(FormatHex(frame.NativeAddress));

        var module = frame.Filename ?? frame.NativeModule;
        if (!string.IsNullOrEmpty(module))
            sb.Append(" in ").Append(module);

        if (frame.IsManaged)
        {
            if (frame.IlOffset is not null)
                sb.Append(" + IL_").Append(frame.IlOffset.Value.ToString("x4", CultureInfo.InvariantCulture));
            if (frame.Token is not null)
                sb.Append(" token=0x").Append(frame.Token.Value.ToString("x8", CultureInfo.InvariantCulture));
        }
        else if (frame.NativeImageOffset is not null and not 0)
        {
            sb.Append(" + 0x").Append(frame.NativeImageOffset.Value.ToString("x", CultureInfo.InvariantCulture));
        }

        sb.AppendLine();
    }

    private static string FormatHex(ulong? value) =>
        value is null ? "n/a" : "0x" + value.Value.ToString("x", CultureInfo.InvariantCulture);

    private static string DescribeExceptionType(string code) => code.ToLowerInvariant() switch
    {
        "0x05000000" => "ManagedException",
        "0x50000000" => "SIGILL",
        "0x70000000" => "SIGFPE",
        "0x60000000" => "SIGBUS",
        "0x03000000" => "SIGTRAP",
        "0x20000000" => "SIGSEGV",
        "0x02000000" => "SIGTERM",
        "0x30000000" => "SIGABRT",
        "0x00000000" => "UnknownSignal",
        _ => "Other"
    };

    private sealed class RuntimeCrashReportDocument
    {
        public RuntimeCrashPayload? Payload { get; set; }
        public RuntimeCrashParameters? Parameters { get; set; }
    }

    private sealed class RuntimeCrashPayload
    {
        [JsonPropertyName("protocol_version")]
        public string? ProtocolVersion { get; set; }

        public RuntimeCrashConfiguration? Configuration { get; set; }

        [JsonPropertyName("process_name")]
        public string? ProcessName { get; set; }

        public List<RuntimeCrashThread>? Threads { get; set; }
    }

    private sealed class RuntimeCrashConfiguration
    {
        public string? Architecture { get; set; }
        public string? Version { get; set; }
    }

    private sealed class RuntimeCrashParameters
    {
        public string? ExceptionType { get; set; }
    }

    private sealed class RuntimeCrashThread
    {
        [JsonPropertyName("is_managed")]
        public bool IsManaged { get; set; }

        public bool Crashed { get; set; }

        [JsonPropertyName("managed_exception_object")]
        public ulong? ManagedExceptionObject { get; set; }

        [JsonPropertyName("managed_exception_type")]
        public string? ManagedExceptionType { get; set; }

        [JsonPropertyName("managed_exception_hresult")]
        public int? ManagedExceptionHResult { get; set; }

        [JsonPropertyName("native_thread_id")]
        public ulong? NativeThreadId { get; set; }

        public RuntimeCrashContext? Ctx { get; set; }

        [JsonPropertyName("stack_frames")]
        public List<RuntimeCrashStackFrame>? StackFrames { get; set; }
    }

    private sealed class RuntimeCrashContext
    {
        [JsonPropertyName("IP")]
        public ulong? Ip { get; set; }

        [JsonPropertyName("SP")]
        public ulong? Sp { get; set; }

        [JsonPropertyName("BP")]
        public ulong? Bp { get; set; }
    }

    private sealed class RuntimeCrashStackFrame
    {
        [JsonPropertyName("is_managed")]
        public bool IsManaged { get; set; }

        [JsonPropertyName("module_address")]
        public ulong? ModuleAddress { get; set; }

        [JsonPropertyName("stack_pointer")]
        public ulong? StackPointer { get; set; }

        [JsonPropertyName("native_address")]
        public ulong? NativeAddress { get; set; }

        [JsonPropertyName("native_offset")]
        public ulong? NativeOffset { get; set; }

        [JsonPropertyName("native_image_offset")]
        public ulong? NativeImageOffset { get; set; }

        public uint? Token { get; set; }

        [JsonPropertyName("il_offset")]
        public uint? IlOffset { get; set; }

        [JsonPropertyName("method_name")]
        public string? MethodName { get; set; }

        [JsonPropertyName("unmanaged_name")]
        public string? UnmanagedName { get; set; }

        public string? Filename { get; set; }

        [JsonPropertyName("native_module")]
        public string? NativeModule { get; set; }

        public int? Repeated { get; set; }

        [JsonPropertyName("repeated_frames")]
        public List<RuntimeCrashStackFrame>? RepeatedFrames { get; set; }
    }

    /// <summary>createdump writes booleans as JSON strings ("true"/"false").</summary>
    private sealed class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.String when bool.TryParse(reader.GetString(), out var b) => b,
                JsonTokenType.String when reader.GetString() is "1" => true,
                JsonTokenType.String when reader.GetString() is "0" => false,
                JsonTokenType.Number => reader.GetInt32() != 0,
                _ => throw new JsonException($"Unexpected token {reader.TokenType} for bool")
            };

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
            writer.WriteBooleanValue(value);
    }

    private sealed class FlexibleUInt64Converter : JsonConverter<ulong?>
    {
        public override ulong? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.Number when reader.TryGetUInt64(out var n) => n,
                JsonTokenType.String when TryParseUInt64(reader.GetString(), out var n) => n,
                _ => throw new JsonException($"Unexpected token {reader.TokenType} for ulong")
            };

        public override void Write(Utf8JsonWriter writer, ulong? value, JsonSerializerOptions options)
        {
            if (value is null) writer.WriteNullValue();
            else writer.WriteNumberValue(value.Value);
        }
    }

    private sealed class FlexibleInt32Converter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.Number when reader.TryGetInt32(out var n) => n,
                JsonTokenType.String when TryParseInt32(reader.GetString(), out var n) => n,
                _ => throw new JsonException($"Unexpected token {reader.TokenType} for int")
            };

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value is null) writer.WriteNullValue();
            else writer.WriteNumberValue(value.Value);
        }
    }

    private sealed class FlexibleUInt32Converter : JsonConverter<uint?>
    {
        public override uint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.Null => null,
                JsonTokenType.Number when reader.TryGetUInt32(out var n) => n,
                JsonTokenType.String when TryParseUInt32(reader.GetString(), out var n) => n,
                _ => throw new JsonException($"Unexpected token {reader.TokenType} for uint")
            };

        public override void Write(Utf8JsonWriter writer, uint? value, JsonSerializerOptions options)
        {
            if (value is null) writer.WriteNullValue();
            else writer.WriteNumberValue(value.Value);
        }
    }

    private static bool TryParseUInt64(string? text, out ulong value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        text = text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return ulong.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        return ulong.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseInt32(string? text, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        text = text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return int.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseUInt32(string? text, out uint value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        text = text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            return uint.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        return uint.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }
}
