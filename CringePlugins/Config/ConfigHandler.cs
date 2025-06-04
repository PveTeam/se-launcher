using System.Text.Json;
using System.Text.Json.Nodes;
using CringePlugins.Config.Spec;
using Json.Schema;
using NLog;
using NuGet;

namespace CringePlugins.Config;

public sealed class ConfigHandler
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly DirectoryInfo _configDirectory;
    private readonly JsonSerializerOptions _serializerOptions = new(NuGetClient.SerializerOptions)
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly EvaluationOptions _evaluationOptions = new()
    {
        OutputFormat = OutputFormat.List,
        RequireFormatValidation = true
    };

    public event EventHandler<ConfigValue>? ConfigReloaded;

    internal ConfigHandler(DirectoryInfo configDirectory)
    {
        _configDirectory = configDirectory;
    }

    public ConfigReference<T> RegisterConfig<T>(string name, T? defaultInstance = null) where T : class
    {
        var spec = IConfigurationSpecProvider.FromType(typeof(T));

        var path = Path.Join(_configDirectory.FullName, $"{name}.json");
        var backupPath = path + $".bak.{DateTimeOffset.Now.ToUnixTimeSeconds()}";

        JsonNode? jsonNode = null;
        if (File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            try
            {
                jsonNode = JsonNode.Parse(stream, new JsonNodeOptions { PropertyNameCaseInsensitive = true },
                    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            }
            catch (JsonException e)
            {
                Log.Warn(e, "Failed to load config {Name}", name);
            }
        }

        var reference = new ConfigReference<T>(name, this);
        if (jsonNode == null || (spec != null && !TryValidate(name, spec, jsonNode)))
        {
            if (File.Exists(path))
                File.Move(path, backupPath);
            defaultInstance ??= Activator.CreateInstance<T>();
            RegisterChange(name, defaultInstance);
            return reference;
        }

        T instance;
        try
        {
            instance = jsonNode.Deserialize<T>(_serializerOptions)!;
        }
        catch (JsonException e)
        {
            Log.Warn(e, "Failed to load config {Name}", name);

            instance = defaultInstance ?? Activator.CreateInstance<T>();
        }
        ConfigReloaded?.Invoke(this, new ConfigValue<T>(name, instance));

        return reference;
    }

    internal void RegisterChange<T>(string name, T newValue)
    {
        var spec = IConfigurationSpecProvider.FromType(typeof(T));

        var jsonNode = JsonSerializer.SerializeToNode(newValue, _serializerOptions)!;

        if (spec != null && !TryValidate(name, spec, jsonNode))
            throw new JsonException($"Supplied config value for {name} is invalid");

        var path = Path.Join(_configDirectory.FullName, $"{name}.json");

        using var stream = File.Create(path);
        using var writer = new Utf8JsonWriter(stream, new()
        {
            Indented = true
        });
        jsonNode.WriteTo(writer, _serializerOptions);

        ConfigReloaded?.Invoke(this, new ConfigValue<T>(name, newValue));
    }

    private bool TryValidate(string name, JsonSchema schema, JsonNode jsonNode)
    {
        var results = schema.Evaluate(jsonNode, _evaluationOptions);

        if (results.IsValid)
            return true;

        Log.Error("Config {Name} is invalid:", name);
        foreach (var detail in results.Details)
        {
            Log.Error("Property {PropertyPath} is invalid:", detail.EvaluationPath);
            foreach (var error in detail.Errors?.Values ?? [])
            {
                Log.Error("\t- {Error}", error);
            }
        }

        return false;
    }

    public abstract record ConfigValue(string Name);
    public sealed record ConfigValue<T>(string Name, T Value) : ConfigValue(Name);
}

public sealed class ConfigReference<T> : IDisposable
{
    private readonly string _name;
    private readonly ConfigHandler _instance;
    private T? _value;

    public T Value
    {
        get => _value ?? throw new InvalidOperationException("Config has not been loaded yet");
        set => _instance.RegisterChange(_name, value);
    }

    internal ConfigReference(string name, ConfigHandler instance)
    {
        _name = name;
        _instance = instance;
        instance.ConfigReloaded += InstanceOnConfigReloaded;
    }

    private void InstanceOnConfigReloaded(object? sender, ConfigHandler.ConfigValue e)
    {
        if (e.Name != _name) return;
        if (e is ConfigHandler.ConfigValue<T> configValue)
            _value = configValue.Value;
    }

    public void Dispose()
    {
        _instance.ConfigReloaded -= InstanceOnConfigReloaded;
    }

    public static implicit operator T(ConfigReference<T> reference) => reference.Value;
}