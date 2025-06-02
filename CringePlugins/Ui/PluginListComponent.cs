using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Serialization;
using CringePlugins.Abstractions;
using CringePlugins.Compatability;
using CringePlugins.Config;
using CringePlugins.Loader;
using CringePlugins.Resolver;
using CringePlugins.Utils;
using ImGuiNET;
using NLog;
using NuGet;
using NuGet.Models;
using NuGet.Versioning;
using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using static ImGuiNET.ImGui;

namespace CringePlugins.Ui;

internal class PluginListComponent : IRenderComponent
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private ImmutableDictionary<string, VersionRange> _packages;
    private ImmutableDictionary<NuGetClient, SearchResult>? _searchResults;
    private bool _searchResultsDirty;
    private string _searchQuery = "";
    private Task? _searchTask;

    private bool _changed;
    private bool _open = true;
    private PackagesConfig _packagesConfig;
    private readonly PackageSourceMapping _sourceMapping;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private ImmutableHashSet<PackageSource>? _selectedSources;
    private readonly string _configPath;
    private readonly string _gameFolder;
    private ImmutableArray<PluginInstance> _plugins;
    private (SearchResultEntry entry, NuGetClient client)? _selected;
    private (PackageSource source, int index)? _selectedSource;

    public PluginListComponent(PackagesConfig packagesConfig, PackageSourceMapping sourceMapping, string configPath, string gameFolder,
        ImmutableArray<PluginInstance> plugins)
    {
        _packagesConfig = packagesConfig;
        _sourceMapping = sourceMapping;
        _configPath = configPath;
        _gameFolder = gameFolder;
        _plugins = plugins;
        _packages = packagesConfig.Packages.ToImmutableDictionary(b => b.Id, b => b.Range,
            StringComparer.OrdinalIgnoreCase);

        MyScreenManager.ScreenAdded += ScreenChanged;
        MyScreenManager.ScreenRemoved += ScreenChanged;
    }

    private void ScreenChanged(MyGuiScreenBase screen)
    {
        _open = MyScreenManager.GetScreenWithFocus() is MyGuiScreenMainMenu && MyGuiScreenGamePlay.Static is null;
    }

    public void OnFrame()
    {
        if (!_open) return;

        SetNextWindowSize(new(700, 500), ImGuiCond.FirstUseEver);

        if (!Begin("Plugin List"))
        {
            End();
            return;
        }

        if (_changed)
        {
            TextDisabled("Changes would be applied on the next restart");
            SameLine();
            if (Button("Restart Now"))
            {
                Process.Start("explorer.exe", "steam://rungameid/244850");
                Process.GetCurrentProcess().Kill();
            }
        }

        if (BeginTabBar("Main"))
        {
            if (BeginTabItem("Installed Plugins"))
            {
                if (BeginTable("InstalledTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable))
                {
                    TableSetupColumn("Id", ImGuiTableColumnFlags.None, .5f, (uint)Columns.Id);
                    TableSetupColumn("Version", ImGuiTableColumnFlags.None, .25f, (uint)Columns.Version);
                    TableSetupColumn("Source", ImGuiTableColumnFlags.None, .25f, (uint)Columns.Source);
                    TableHeadersRow();

                    var sortSpecs = TableGetSortSpecs();

                    if (sortSpecs.SpecsDirty)
                    {
                        _plugins = _plugins.Sort((x, y) => ComparePlugins(x, y, sortSpecs));
                        sortSpecs.SpecsDirty = false;
                    }

                    foreach (var plugin in _plugins)
                    {
                        TableNextRow();

                        TableNextColumn();
                        BeginDisabled(!plugin.HasConfig);
                        if (plugin.WrappedInstance?.LastException is not null)
                            PushStyleColor(plugin.HasConfig ? ImGuiCol.Text : ImGuiCol.TextDisabled,
                                plugin.HasConfig ? Color.Red.ToFloat4() : Color.DarkRed.ToFloat4());
                        if (Selectable(plugin.Metadata.Name, false, ImGuiSelectableFlags.SpanAllColumns))
                            plugin.OpenConfig();
                        if (plugin.WrappedInstance?.LastException is not null)
                        {
                            PopStyleColor();
                            if (IsItemHovered(ImGuiHoveredFlags.ForTooltip))
                                SetTooltip(
                                    $"{plugin.WrappedInstance.LastException.GetType()}: {plugin.WrappedInstance.LastException.Message}");
                        }
                        EndDisabled();
                        TableNextColumn();
                        Text(plugin.Metadata.Version.ToString());
                        TableNextColumn();
                        Text(plugin.Metadata.Source);
                    }

                    EndTable();
                }

                EndTabItem();
            }

            if (BeginTabItem("Browse Plugins"))
            {
                AvailablePluginsTab();

                EndTabItem();
            }

            if (BeginTabItem("Sources Configuration"))
            {
                BeginChild("Sources List", new(400, 0), ImGuiChildFlags.Border | ImGuiChildFlags.ResizeX);
                {
                    if (BeginTable("Sources Table", 2,
                            ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp))
                    {
                        TableSetupColumn("Name", ImGuiTableColumnFlags.None, .2f);
                        TableSetupColumn("Url", ImGuiTableColumnFlags.None, .8f);
                        TableHeadersRow();

                        for (var index = 0; index < _packagesConfig.Sources.Length; index++)
                        {
                            var source = _packagesConfig.Sources[index];
                            TableNextRow();
                            
                            TableNextColumn();

                            if (Selectable(source.Name, index == _selectedSource?.index, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                _selectedSource = (source, index);
                            }

                            TableNextColumn();

                            Text(source.Url);
                        }

                        EndTable();
                    }
                    
                    EndChild();
                }
                
                SameLine();
                
                BeginGroup();

                BeginChild("Source View", new(0, -GetFrameHeightWithSpacing())); // Leave room for 1 line below us
                {
                    if (_selectedSource is not null)
                    {
                        var (selectedSource, index) = _selectedSource.Value;
                        
                        var name = selectedSource.Name;
                        if (InputText("Name", ref name, 256))
                            selectedSource = selectedSource with
                            {
                                Name = name
                            };

                        var url = selectedSource.Url;
                        if (InputText("Url", ref url, 1024))
                            selectedSource = selectedSource with
                            {
                                Url = url
                            };
                        
                        var pattern = selectedSource.Pattern;
                        if (InputText("Pattern", ref pattern, 1024))
                            selectedSource = selectedSource with
                            {
                                Pattern = pattern
                            };

                        _selectedSource = (selectedSource, index);

                        if (Button("Save"))
                        {
                            var array = _packagesConfig.Sources.RemoveAt(index).Insert(index, selectedSource);

                            _packagesConfig = _packagesConfig with
                            {
                                Sources = array
                            };

                            _selectedSource = null;
                            
                            Save();
                        }
                        
                        SameLine();

                        if (Button("Delete"))
                        {
                            var array = _packagesConfig.Sources.RemoveAt(index);

                            _packagesConfig = _packagesConfig with
                            {
                                Sources = array
                            };

                            _selectedSource = null;
                            
                            Save();
                        }
                    }
                    
                    EndChild();
                }

                if (Button("Add New"))
                {
                    var source = new PackageSource("source name", "", "https://url.to/index.json");
                    
                    var array = _packagesConfig.Sources.Add(source);

                    _packagesConfig = _packagesConfig with
                    {
                        Sources = array
                    };

                    _selectedSource = (source, array.Length - 1);
                }
                
                EndGroup();
                
                EndTabItem();
            }

            if (BeginTabItem("Settings"))
            {
                var oldConfigPath = Path.Join(_gameFolder, "Plugins", "config.xml");
                if (File.Exists(oldConfigPath))
                {
                    if (Button("Migrate PluginLoader Plugins"))
                    {
                        var configSerializer = new XmlSerializer(typeof(PluginLoaderConfig));
                        using var fs = File.OpenRead(oldConfigPath);

                        if (configSerializer.Deserialize(fs) is PluginLoaderConfig oldConfig)
                        {
                            _packagesConfig = oldConfig.MigratePlugins(_packagesConfig);

                            Save(false);

                            _packages = _packagesConfig.Packages.ToImmutableDictionary(b => b.Id, b => b.Range, StringComparer.OrdinalIgnoreCase);
                        }
                    }

                    var hasModLodaer = _packages.ContainsKey("Plugin.ClientModLoader");

                    if (!hasModLodaer)
                        BeginDisabled();

                    if (Button("Migrate Pluginloader Mods"))
                    {
                        var configSerializer = new XmlSerializer(typeof(PluginLoaderConfig));
                        using var fs = File.OpenRead(oldConfigPath);

                        if (configSerializer.Deserialize(fs) is PluginLoaderConfig plConfig)
                        {
                            var dir = new DirectoryInfo(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "CringeLauncher"));
                            var file = Path.Join(dir.FullName, "mods.json");

                            using var modsFile = File.Create(file);
                            JsonSerializer.Serialize(modsFile, plConfig.GetMods(), _serializerOptions);
                        }
                    }

                    if (!hasModLodaer)
                    {
                        if (IsItemHovered())
                            SetTooltip("Requires Plugin.ClientModLoader");

                        EndDisabled();
                    }
                }
                EndTabItem();
            }

            EndTabBar();
        }

        End();
    }

    // TODO sources editor
    // TODO combobox with active sources (to limit search results to specific list of sources)
    private unsafe void AvailablePluginsTab()
    {
        if (_searchResults is null && _searchTask is null)
        {
            _searchTask = RefreshAsync();
        }

        InputText("##searchbox", ref _searchQuery, 256);

        SameLine();

        if (Button("Search"))
        {
            _searchTask = RefreshAsync();
            return;
        }

        SameLine();

        if (BeginCombo("##sources",
                _selectedSources is null ? "All Sources" :
                _selectedSources.Count > 2 ? $"{_selectedSources.First().Name} +{_selectedSources.Count - 1}" :
                string.Join(",", _selectedSources.Select(b => b.Name)), ImGuiComboFlags.WidthFitPreview))
        {
            foreach (var source in _packagesConfig.Sources)
            {
                var selected = _selectedSources?.Contains(source) ?? true;
                if (Selectable(source.Name, ref selected))
                {
                    _selectedSources = selected
                        ? (_selectedSources?.Count ?? 0) + 1 == _packagesConfig.Sources.Length ? null : _selectedSources?.Add(source)
                        : (_selectedSources ?? _packagesConfig.Sources.ToImmutableHashSet()).Remove(source);
                    
                    _searchTask = RefreshAsync();
                    return;
                }
            }

            EndCombo();
        }
        
        Spacing();
        
        switch (_searchTask)
        {
            case { IsCompleted: false }:
                TextDisabled("Loading...");
                return;
            case { IsCompletedSuccessfully: false }:
            {
                TextDisabled("Failed to load plugins list");
                if (_searchTask.Exception is null) return;

                foreach (var exception in _searchTask.Exception.InnerExceptions)
                {
                    TextWrapped($"{exception.GetType()}: {exception.Message}");
                }

                return;
            }
        }

        var searchResults = _searchResults ?? ImmutableDictionary<NuGetClient, SearchResult>.Empty;

        if (searchResults.IsEmpty || searchResults.Values.All(b => b.Entries.IsEmpty))
        {
            TextDisabled("Nothing found");
            return;
        }

        BeginChild("List", new(400, 0), ImGuiChildFlags.Border | ImGuiChildFlags.ResizeX);
        {
            if (BeginTable("AvailableTable", 3,
                    ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Sortable))
            {
                TableSetupColumn("Id", ImGuiTableColumnFlags.None, .5f, (uint)Columns.Id);
                TableSetupColumn("Version", ImGuiTableColumnFlags.None, .25f, (uint)Columns.Version);
                TableSetupColumn("Installed", ImGuiTableColumnFlags.None, .25f, (uint)Columns.Installed);
                TableHeadersRow();

                var sortSpecs = TableGetSortSpecs();

                if (_searchResultsDirty || sortSpecs.SpecsDirty)
                {
                    var builder = searchResults.ToBuilder();
                    foreach (var kvp in builder.ToArray())
                    {
                        builder[kvp.Key] = kvp.Value with
                        {
                            Entries = kvp.Value.Entries.Sort(
                                (x, y) => SortSearchResults(x, y, _packages.ContainsKey(x.Id), _packages.ContainsKey(y.Id), sortSpecs))
                        };
                    }

                    _searchResults = builder.ToImmutable();
                    searchResults = _searchResults;

                    _searchResultsDirty = false;
                    sortSpecs.SpecsDirty = false;
                }

                foreach (var (client, result) in searchResults)
                {
                    foreach (var package in result.Entries.Take(100))
                    {
                        TableNextRow();

                        TableNextColumn();

                        var selected = _selected?.entry.Id.Equals(package.Id, StringComparison.OrdinalIgnoreCase) ==
                                       true;

                        if (Selectable(package.Title ?? package.Id, ref selected, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            _selected = selected ? (package, client) : null;
                        }

                        if (!string.IsNullOrEmpty(package.Summary) && IsItemHovered(ImGuiHoveredFlags.ForTooltip))
                        {
                            SetTooltip(package.Summary);
                        }

                        TableNextColumn();
                        Text(package.Version.ToString());

                        TableNextColumn();

                        var installed = _packages.ContainsKey(package.Id);
                        TextColored(installed ? new(0f, 1f, 0f, 1f) : new(1f, 0f, 0f, 1f),
                            installed ? "Installed" : "Not Installed");
                    }
                }

                EndTable();
            }

            EndChild();
        }

        SameLine();

        BeginGroup();

        BeginChild("Package View", new(0, -GetFrameHeightWithSpacing())); // Leave room for 1 line below us

        if (_selected is not null)
        {
            var selected = _selected.Value.entry;

            Text(selected.Title ?? selected.Id);
            SameLine();
            TextColored(*GetStyleColorVec4(ImGuiCol.TextLink), selected.Version.ToString());
            Separator();
            if (BeginTabBar("##PackageViewTabs"))
            {
                if (BeginTabItem("Description"))
                {
                    TextWrapped(selected.Description ?? "Nothing.");
                    EndTabItem();
                }

                if (BeginTabItem("Details"))
                {
                    Text("Pulled from");
                    SameLine();
                    var url = _selected.Value.client.ToString();
                    TextLinkOpenURL(_packagesConfig.Sources.FirstOrDefault(b => b.Url == url)?.Name ?? url, url);

                    if (selected.Authors is not null)
                    {
                        Text("Authors:");
                        SameLine();
                        TextWrapped(selected.Authors.Author);
                    }

                    if (selected.TotalDownloads.HasValue)
                    {
                        Text("Total downloads:");
                        SameLine();
                        TextColored(*GetStyleColorVec4(ImGuiCol.TextLink), selected.TotalDownloads.ToString());
                    }

                    if (!string.IsNullOrEmpty(selected.ProjectUrl))
                        TextLinkOpenURL("Project URL", selected.ProjectUrl);

                    EndTabItem();
                }

                EndTabBar();
            }
        }

        EndChild();

        if (_selected is not null)
        {
            var selected = _selected.Value.entry;

            var installed = _packages.ContainsKey(selected.Id);
            if (Button(installed ? "Uninstall" : "Install"))
            {
                _packages = installed
                    ? _packages.Remove(selected.Id)
                    : _packages.Add(selected.Id, new(selected.Version));

                Save();
            }
        }

        EndGroup();
    }

    private async Task RefreshAsync()
    {
        _searchResults = null;
        _searchResultsDirty = true;

        var builder = ImmutableDictionary.CreateBuilder<NuGetClient, SearchResult>();

        await foreach (var source in _sourceMapping)
        {
            if (source == null || _selectedSources is not null && _selectedSources.All(b => b.Url != source.ToString()))
                continue;
            
            try
            {
                var result = await source.SearchPackagesAsync(_searchQuery, take: 1000, packageType: "CringePlugin");

                builder.Add(source, result);
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to get packages from source {Source}", source);
            }
        }

        _searchResults = builder.ToImmutable();
    }

    private void Save(bool keepPackages = true)
    {
        _changed = true;

        using var stream = File.Create(_configPath);

        JsonSerializer.Serialize(stream, keepPackages ? _packagesConfig with
        {
            Packages = [.._packages.Select(b => new PackageReference(b.Key, b.Value))]
        } : _packagesConfig, NuGetClient.SerializerOptions);
    }

    private static unsafe int ComparePlugins(PluginInstance x, PluginInstance y, ImGuiTableSortSpecsPtr specs)
    {
        ImGuiTableColumnSortSpecs* ptr = specs.Specs;
        for (var i = 0; i < specs.SpecsCount; i++)
        {
            var spec = ptr[i];
            var delta = 0;

            switch ((Columns)spec.ColumnUserID)
            {
                case Columns.Id:
                    delta = string.Compare(x.Metadata.Name, y.Metadata.Name, StringComparison.OrdinalIgnoreCase);
                    break;
                case Columns.Version:
                    delta = x.Metadata.Version.CompareTo(y.Metadata.Version);
                    break;
                case Columns.Source:
                    delta = string.Compare(x.Metadata.Source, y.Metadata.Source, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            if (delta > 0)
                return spec.SortDirection == ImGuiSortDirection.Descending ? -1 : 1;

            if (delta < 0)
                return spec.SortDirection == ImGuiSortDirection.Descending ? 1 : -1;
        }

        //default
        return string.Compare(x.Metadata.Name, y.Metadata.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static unsafe int SortSearchResults(SearchResultEntry x, SearchResultEntry y, bool xInstall, bool yInstall,
        ImGuiTableSortSpecsPtr specs)
    {
        ImGuiTableColumnSortSpecs* ptr = specs.Specs;
        for (var i = 0; i < specs.SpecsCount; i++)
        {
            var spec = ptr[i];
            var delta = 0;

            switch ((Columns)spec.ColumnUserID)
            {
                case Columns.Id:
                    delta = string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);
                    break;
                case Columns.Version:
                    delta = x.Version.CompareTo(y.Version);
                    break;
                case Columns.Installed:
                    delta = xInstall.CompareTo(yInstall);
                    break;
            }

            if (delta > 0)
                return spec.SortDirection == ImGuiSortDirection.Descending ? -1 : 1;

            if (delta < 0)
                return spec.SortDirection == ImGuiSortDirection.Descending ? 1 : -1;
        }

        //default
        return string.Compare(x.Title, y.Title, StringComparison.OrdinalIgnoreCase);
    }

    private enum Columns : uint
    {
        Id,
        Version,
        Source,
        Installed
    }
}