using CringeBootstrap.Abstractions;
using CringePlugins.Abstractions;
using CringePlugins.Compatability;
using CringePlugins.Config;
using CringePlugins.Loader;
using CringePlugins.Resolver;
using CringePlugins.Services;
using CringePlugins.Utils;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NuGet;
using NuGet.Models;
using NuGet.Versioning;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using System.Xml.Serialization;
using static ImGuiNET.ImGui;

namespace CringePlugins.Ui;

internal class PluginListComponent : IRenderComponent
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private ImmutableDictionary<string, VersionRange> _packages;
    private ImmutableArray<(NuGetClient? Client, SearchResult Entry)>? _searchResults;
    private bool _searchResultsDirty;
    private string _searchQuery = "";
    private Task? _searchTask;

    private string _profileSearch = "";
    private string _newProfileName = "";
    private int _selectedProfile = -1;
    private ImmutableArray<Profile> _profiles;

    private readonly DirectoryInfo _dataDir;
    private readonly DirectoryInfo _cacheDir;
    private ImmutableDictionary<string, CachedPackage> _loadedPackages;
    private bool _disableUpdates;
    private bool _disablePluginUpdates;
    private bool _usePreviewBranch;
    private bool _cacheModAssemblies;
    private bool _cacheScriptAssemblies;

    private bool _showLocalSource;
    private bool _useLocalSource;

    private bool _restartRequired;
    private bool _open = true;
    private readonly ConfigReference<PackagesConfig> _packagesConfig;
    private readonly ConfigReference<LauncherConfig> _launcherConfig;
    private readonly PackageSourceMapping _sourceMapping;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private ImmutableHashSet<PackageSource>? _selectedSources;
    private readonly string _gameFolder;
    private ImmutableArray<PluginInstance> _plugins;
    private (SearchResultEntry entry, NuGetClient? client)? _selected;
    private (PackageSource source, int index)? _selectedSource;
    private readonly IImGuiImageService _imageService = GameServicesExtension.GameServices.GetRequiredService<IImGuiImageService>();
    private readonly ICorePlugin _corePlugin = GameServicesExtension.GameServices.GetRequiredService<ICorePlugin>();

    public PluginListComponent(ConfigReference<PackagesConfig> packagesConfig,
        ConfigReference<LauncherConfig> launcherConfig,
        PackageSourceMapping sourceMapping, string gameFolder, ImmutableHashSet<PluginInstance> plugins,
        DirectoryInfo dataDir, DirectoryInfo cacheDir, IReadOnlyDictionary<string, CachedPackage> loadedPackages)
    {
        _packagesConfig = packagesConfig;
        _launcherConfig = launcherConfig;
        _sourceMapping = sourceMapping;
        _gameFolder = gameFolder;
        _plugins = [..plugins];
        _packages = packagesConfig.Value.Packages.ToImmutableDictionary(b => b.Id, b => b.Range,
            StringComparer.OrdinalIgnoreCase);
        _profiles = packagesConfig.Value.Profiles;

        _disablePluginUpdates = _launcherConfig.Value.DisablePluginUpdates;
        _disableUpdates = _launcherConfig.Value.DisableLauncherUpdates;
        _usePreviewBranch = _launcherConfig.Value.UsePreviewBranch;
        _cacheModAssemblies = _launcherConfig.Value.CacheModAssemblies;
        _cacheScriptAssemblies = _launcherConfig.Value.CacheScriptAssemblies;

        _dataDir = dataDir;
        _cacheDir = cacheDir;
        _loadedPackages = loadedPackages.ToImmutableDictionary();

        MyScreenManager.ScreenAdded += ScreenChanged;
        MyScreenManager.ScreenRemoved += ScreenChanged;
    }

    private void ScreenChanged(MyGuiScreenBase screen)
    {
        _open = MyScreenManager.GetScreenWithFocus() is MyGuiScreenMainMenu;
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

        if (_restartRequired)
        {
            TextDisabled("Changes will be applied on the next restart");
            SameLine();
            if (Button("Restart Now"))
            {
                _corePlugin.Restart();
            }
        }

        if (BeginTabBar("Main"))
        {
            if (BeginTabItem("Installed Plugins"))
            {
                if (BeginTable("InstalledTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable))
                {
                    TableSetupColumn("Id", ImGuiTableColumnFlags.WidthStretch, .5f, (uint)Columns.Id);
                    TableSetupColumn("Version", ImGuiTableColumnFlags.WidthStretch, .25f, (uint)Columns.Version);
                    TableSetupColumn("Source", ImGuiTableColumnFlags.WidthStretch, .25f, (uint)Columns.Source);
                    TableHeadersRow();

                    var sortSpecs = TableGetSortSpecs();

                    if (sortSpecs.SpecsDirty)
                    {
                        _plugins = _plugins.Sort((x, y) => ComparePlugins(x, y, sortSpecs));
                        sortSpecs.SpecsDirty = false;
                    }

                    var i = 0;
                    foreach (var plugin in _plugins)
                    {
                        TableNextRow();

                        TableNextColumn();
                        if (plugin.WrappedInstance?.LastException is not null)
                        {
                            var color = plugin.IsReloading ? Color.Yellow : plugin.HasConfig ? Color.Red : Color.DarkRed;
                            PushStyleColor(ImGuiCol.Text, color.ToFloat4());
                        }
                        else if (!plugin.HasConfig)
                        {
                            unsafe
                            {
                                PushStyleColor(ImGuiCol.Text, *GetStyleColorVec4(ImGuiCol.TextDisabled));
                            }
                        }

                        if (Selectable($"{plugin.Metadata.Name}##{++i}", false, ImGuiSelectableFlags.SpanAllColumns) &&
                            plugin is { IsReloading: false, HasConfig: true })
                            plugin.OpenConfig();

                        if (plugin is { IsReloading: false, IsLocal: true } && BeginPopupContextItem($"##{plugin.Metadata.Name}ContextMenu{i}"))
                        {
                            if (Button($"Reload##{i}"))
                            {
                                PluginsLifetime.ReloadPluginAsync(plugin).ConfigureAwait(false);
                            }
                            EndPopup();
                        }

                        if (plugin.WrappedInstance?.LastException is not null)
                        {
                            PopStyleColor();
                            if (IsItemHovered(ImGuiHoveredFlags.ForTooltip) && !plugin.IsReloading)
                                SetTooltip(
                                    $"{plugin.WrappedInstance.LastException.GetType()}: {plugin.WrappedInstance.LastException.Message}");
                        } 
                        else if (!plugin.HasConfig)
                        {
                            PopStyleColor();
                        }

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

            if (BeginTabItem("Profiles"))
            {
                ProfilesTab();
                EndTabItem();
            }

            if (BeginTabItem("Sources Configuration"))
            {
                BeginChild("Sources List", new(400, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
                {
                    if (BeginTable("Sources Table", 2,
                            ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp))
                    {
                        TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, .2f);
                        TableSetupColumn("Url", ImGuiTableColumnFlags.WidthStretch, .8f);
                        TableHeadersRow();

                        for (var index = 0; index < _packagesConfig.Value.Sources.Length; index++)
                        {
                            var source = _packagesConfig.Value.Sources[index];
                            TableNextRow();

                            TableNextColumn();

                            if (Selectable($"{source.Name}##{index}", index == _selectedSource?.index, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                _selectedSource = (source, index);
                            }

                            if (IsItemActive() && !IsItemHovered())
                            {
                                var nextIndex = index + (GetMouseDragDelta(0).Y < 0 ? -1 : 1);
                                if (nextIndex >= 0 && nextIndex < _packagesConfig.Value.Sources.Length)
                                {
                                    var builder = _packagesConfig.Value.Sources.ToBuilder();

                                    builder[index] = builder[nextIndex];
                                    builder[nextIndex] = source;

                                    _packagesConfig.Value = _packagesConfig.Value with
                                    {
                                        Sources = builder.ToImmutable()
                                    };
                                    _selectedSource = (source, nextIndex);
                                }
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
                            var array = _packagesConfig.Value.Sources.RemoveAt(index).Insert(index, selectedSource);

                            _packagesConfig.Value = _packagesConfig.Value with
                            {
                                Sources = array
                            };

                            _selectedSource = null;

                            Save();
                        }

                        SameLine();

                        if (Button("Delete"))
                        {
                            var array = _packagesConfig.Value.Sources.RemoveAt(index);

                            _packagesConfig.Value = _packagesConfig.Value with
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

                    var array = _packagesConfig.Value.Sources.Add(source);

                    _packagesConfig.Value = _packagesConfig.Value with
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
                if (Checkbox("Disable Plugin Updates", ref _disablePluginUpdates))
                {
                    _launcherConfig.Value = _launcherConfig.Value with { DisablePluginUpdates = _disablePluginUpdates };
                }
                if (Checkbox("Disable Launcher Updates", ref _disableUpdates))
                {
                    _launcherConfig.Value = _launcherConfig.Value with { DisableLauncherUpdates = _disableUpdates };
                }

                if (_usePreviewBranch)
                {
                    TextColored(new(1, 0, 0, 1), "Preview Branch is unstable! Use at your own risk!");
                }
                if (Checkbox("Use Preview Branch", ref _usePreviewBranch))
                {
                    _launcherConfig.Value = _launcherConfig.Value with
                    {
                        UsePreviewBranch = _usePreviewBranch
                    };
                }
                if (IsItemHovered(ImGuiHoveredFlags.ForTooltip))
                {
                    SetTooltip("Preview branch is used for testing new features and bug fixes. It may be unstable and may not work as expected.");
                }
                
                if (Checkbox("Cache Mod Assemblies", ref _cacheModAssemblies))
                {
                    _launcherConfig.Value = _launcherConfig.Value with
                    {
                        CacheModAssemblies = _cacheModAssemblies
                    };
                }
                BeginDisabled(!_cacheModAssemblies);

                SameLine();
                if (Button("Clear Mod Assembly Cache"))
                {
                    var dir = Path.Join(_dataDir.FullName, "cache", "mods");
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }

                EndDisabled();
                if (Checkbox("Cache Script Assemblies", ref _cacheScriptAssemblies))
                {
                    _launcherConfig.Value = _launcherConfig.Value with
                    {
                        CacheScriptAssemblies = _cacheScriptAssemblies
                    };
                }

                BeginDisabled(!_cacheScriptAssemblies);

                SameLine();
                if (Button("Clear Script Assembly Cache"))
                {
                    var dir = Path.Join(_dataDir.FullName, "cache", "scripts");
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }

                EndDisabled();

                var oldConfigPath = Path.Join(_gameFolder, "Plugins", "config.xml");
                if (File.Exists(oldConfigPath))
                {
                    if (Button("Migrate PluginLoader Plugins"))
                    {
                        var configSerializer = new XmlSerializer(typeof(PluginLoaderConfig));
                        using var fs = File.OpenRead(oldConfigPath);

                        if (configSerializer.Deserialize(fs) is PluginLoaderConfig oldConfig)
                        {
                            _packagesConfig.Value = oldConfig.MigratePlugins(_packagesConfig);

                            Save(false);

                            _packages = _packagesConfig.Value.Packages.ToImmutableDictionary(b => b.Id, b => b.Range, StringComparer.OrdinalIgnoreCase);
                        }
                    }

                    var hasModLodaer = _packages.ContainsKey("Plugin.ClientModLoader");

                    SameLine();

                    if (!hasModLodaer)
                        BeginDisabled();

                    if (Button("Migrate Pluginloader Mods"))
                    {
                        var configSerializer = new XmlSerializer(typeof(PluginLoaderConfig));
                        using var fs = File.OpenRead(oldConfigPath);

                        if (configSerializer.Deserialize(fs) is PluginLoaderConfig plConfig)
                        {
                            var file = Path.Join(_dataDir.FullName, "mods.json");

                            using var modsFile = File.Create(file);
                            JsonSerializer.Serialize(modsFile, plConfig.GetMods(), _serializerOptions);
                            _restartRequired = true;
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

    private unsafe void ProfilesTab()
    {
        InputText("##searchbox", ref _profileSearch, 256);

        SameLine();

        if (Button("Create New Profile"))
            OpenPopup("New Profile");


        if (IsItemHovered(ImGuiHoveredFlags.ForTooltip))
        {
            SetTooltip("Create a new profile from enabled plugins");
        }

        if (BeginPopupModal("New Profile", ImGuiWindowFlags.AlwaysAutoResize))
        {
            InputText("Name", ref _newProfileName, 50);
            Separator();

            if (Button("Ok##newProfileOk", new Vector2(120, 0)))
            {
                var len = _profiles.Length;
                _profiles = _profiles.Add(new(_newProfileName, [.. _packages.Select(x => new PackageReference(x.Key, x.Value))]));
                _selectedProfile = len;

                _packagesConfig.Value = _packagesConfig.Value with
                {
                    Profiles = _profiles
                };

                CloseCurrentPopup();
            }
            SetItemDefaultFocus();
            SameLine();
            if (Button("Cancel##newProfileCancel", new Vector2(120, 0)))
            {
                CloseCurrentPopup();
            }

            EndPopup();
        }

        Spacing();

        if (_profiles.IsEmpty)
        {
            TextDisabled("No Profiles");
            return;
        }

        BeginChild("Profile List", new(400, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);

        if (BeginTable("ProfilesTable", 2,
                    ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp))
        {
            TableSetupColumn("Id##ProfilesTable", ImGuiTableColumnFlags.WidthStretch, .5f, (uint)Columns.Id);
            TableSetupColumn("Plugins", ImGuiTableColumnFlags.WidthStretch, .25f, (uint)Columns.Count);
            TableHeadersRow();

            for (var i = 0; i < _profiles.Length; i++)
            {
                var(id, plugins) = _profiles[i];

                if (!id.Contains(_profileSearch, StringComparison.OrdinalIgnoreCase))
                    continue;

                TableNextRow();

                TableNextColumn();

                var selected = _selectedProfile == i;

                if (Selectable($"{id}##profiles{i}", ref selected, ImGuiSelectableFlags.SpanAllColumns))
                {
                    _selectedProfile = selected ? i : -1;
                }

                TableNextColumn();
                Text(plugins.Length.ToString());
            }

            EndTable();
        }

        EndChild();

        SameLine();

        BeginGroup();

        BeginChild("Profile View", new(0, -GetFrameHeightWithSpacing())); // Leave room for 1 line below us

        if (_selectedProfile >= 0)
        {
            var (id, plugins) = _profiles[_selectedProfile];

            Text(id);
            Separator();

            if (BeginTable("ProfilePluginsTable", 2, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp))
            {
                TableSetupColumn("Id##pluginProfilesId", ImGuiTableColumnFlags.WidthStretch, .5f, (uint)Columns.Id);
                TableSetupColumn("Version##pluginProfilesVersion", ImGuiTableColumnFlags.WidthStretch, .25f, (uint)Columns.Version);

                foreach (var plugin in plugins)
                {
                    TableNextRow();

                    TableNextColumn();

                    Text(plugin.Id);

                    TableNextColumn();
                    Text(plugin.Range.ToShortString());
                }

                EndTable();
            }
        }
        EndChild();

        if (_selectedProfile >= 0)
        {
            if (Button("Activate"))
            {
                _packages = _profiles[_selectedProfile].Plugins.ToImmutableDictionary(b => b.Id, b => b.Range);

                Save();
            }

            SameLine();
            if (Button("Delete"))
                OpenPopup("Delete?##ProfileDeletePopup");

            if (BeginPopupModal("Delete?##ProfileDeletePopup", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Text("Are you sure you want to delete this profile?");
                Separator();

                if (Button("Yes", new Vector2(120, 0)))
                {
                    _profiles = _profiles.RemoveAt(_selectedProfile);

                    _packagesConfig.Value = _packagesConfig.Value with
                    {
                        Profiles = _profiles
                    };

                    _selectedProfile = -1;

                    CloseCurrentPopup();
                }
                SetItemDefaultFocus();
                SameLine();
                if (Button("No", new Vector2(120, 0)))
                {
                    CloseCurrentPopup();
                }

                EndPopup();
            }
        }

        EndGroup();
    }

    private unsafe void AvailablePluginsTab()
    {
        if (_searchResults is null && _searchTask is null)
        {
            _searchTask = RefreshAsync();
        }

        InputText("##searchbox", ref _searchQuery, 256);

        SameLine();

        if ((IsItemFocused() && IsKeyReleased(ImGuiKey.Enter)) || Button("Search"))
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
            foreach (var source in _packagesConfig.Value.Sources)
            {
                var selected = _selectedSources?.Contains(source) ?? true;
                if (Selectable(source.Name, ref selected))
                {
                    _selectedSources = selected
                        ? (_selectedSources?.Count ?? 0) + 1 == _packagesConfig.Value.Sources.Length ? null : _selectedSources?.Add(source)
                        : (_selectedSources ?? [.. _packagesConfig.Value.Sources]).Remove(source);

                    _searchTask = RefreshAsync();
                    EndCombo();
                    return;
                }
            }

            if (_showLocalSource && Selectable("Offline Packages", ref _useLocalSource))
            {
                _searchTask = RefreshAsync();
                EndCombo();
                return;
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

        var searchResults = _searchResults ?? [];

        if (searchResults.IsEmpty || searchResults.All(b => b.Entry.Entries.IsEmpty))
        {
            TextDisabled("Nothing found");
            return;
        }

        BeginChild("List", new(400, 0), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeX);
        {
            if (BeginTable("AvailableTable", 3,
                    ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.Sortable))
            {
                TableSetupColumn("Id", ImGuiTableColumnFlags.WidthStretch, .5f, (uint)Columns.Id);
                TableSetupColumn("Version", ImGuiTableColumnFlags.WidthStretch, .25f, (uint)Columns.Version);
                TableSetupColumn("Installed", ImGuiTableColumnFlags.WidthStretch, .25f, (uint)Columns.Installed);
                TableHeadersRow();

                var sortSpecs = TableGetSortSpecs();

                if (_searchResultsDirty || sortSpecs.SpecsDirty)
                {
                    var builder = searchResults.ToBuilder();
                    for (var i = 0; i < builder.Count; i++)
                    {
                        builder[i] = (searchResults[i].Client, searchResults[i].Entry with
                        {
                            Entries = searchResults[i].Entry.Entries.Sort(
                                (x, y) => SortSearchResults(x, y, _packages.ContainsKey(x.Id), _packages.ContainsKey(y.Id), sortSpecs))
                        });
                    }


                    searchResults = builder.ToImmutable();
                    _searchResults = searchResults;

                    _searchResultsDirty = false;
                    sortSpecs.SpecsDirty = false;
                }

                var idcounter = 0;
                foreach (var (client, result) in searchResults)
                {
                    foreach (var package in result.Entries)
                    {
                        TableNextRow();

                        TableNextColumn();

                        var selected = _selected?.entry.Id.Equals(package.Id, StringComparison.OrdinalIgnoreCase) ==
                                       true;

                        if (Selectable($"{package.Title ?? package.Id}##{idcounter++}", ref selected, ImGuiSelectableFlags.SpanAllColumns))
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

                        Vector4 col;
                        string statusText;
                        var isSelected = _packages.ContainsKey(package.Id);
                        var isLoaded = _loadedPackages.ContainsKey(package.Id);
                        if (isSelected && isLoaded)
                        {
                            col = new(0f, 1f, 0f, 1f);
                            statusText = "Installed";
                        } 
                        else if (isLoaded || isSelected)
                        {
                            col = new(1f, 1f, 0f, 1f);
                            statusText = isLoaded ? "Transient" : "Queued";
                        }
                        else
                        {
                            col = new(1f, 0f, 0f, 1f);
                            statusText = "Not Installed";
                        }

                        TextColored(col, statusText);
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

            if (!string.IsNullOrEmpty(selected.IconUrl))
            {
                var image = selected.IconUrl.StartsWith("http", StringComparison.Ordinal) ?
                    _imageService.GetFromUrl(new Uri(selected.IconUrl))
                    : _imageService.GetFromPath(selected.IconUrl);
                Image(image, new(64, 64));
                SameLine();
            }

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
                    var url = _selected.Value.client?.ToString();

                    if (!string.IsNullOrEmpty(url))
                        TextLinkOpenURL(_packagesConfig.Value.Sources.FirstOrDefault(b => b.Url == url)?.Name ?? url, url);
                    else
                        TextColored(new(1f, 1f, 0f, 1f), "Local Cache");

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

                // if install state was changed by user we want to always treat it as queued even if it was rolled back
                if (installed)
                    _loadedPackages = _loadedPackages.Remove(selected.Id);

                Save();
            }
        }

        EndGroup();
    }

    private async Task RefreshAsync()
    {
        _searchResults = null;
        _searchResultsDirty = true;

        var builder = ImmutableArray.CreateBuilder<(NuGetClient?, SearchResult)>();

        var localSearch = _useLocalSource ? Task.Run(GetLocalPackages) : null;

        await foreach (var source in _sourceMapping)
        {
            if (source == null)
            {
                if (!_showLocalSource)
                {
                    _showLocalSource = true;
                    _useLocalSource = true;
                    localSearch ??= Task.Run(GetLocalPackages);

                    Log.Warn("Source missing, enabling local cache fallback");
                }
                continue;
            }

            if (_selectedSources?.All(b => b.Url != source.ToString()) == true)
                continue;

            try
            {
                var result = await source.SearchPackagesAsync(_searchQuery, take: 1000, packageType: "CringePlugin");

                builder.Add((source, result));
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to get packages from source {Source}", source);
            }
        }

        if (localSearch != null)
            builder.Add((null, await localSearch));

        _searchResults = builder.ToImmutable();
    }

    private void Save(bool keepChanges = true)
    {
        _packagesConfig.Value = keepChanges ? _packagesConfig.Value with
        {
            Packages = [.. _packages.Select(b => new PackageReference(b.Key, b.Value))]
        } : _packagesConfig;

        _restartRequired = true;
    }

    private SearchResult GetLocalPackages()
    {
        var serializer = new XmlSerializer(typeof(Nuspec));

        var entries = 0;
        var bag = new ConcurrentBag<SearchResultEntry>();

        var searchText = _searchQuery;

        void LoadDir(DirectoryInfo packageDir)
        {
            NuGetVersion? maxVersion = null;
            DirectoryInfo? maxVersionDir = null;
            foreach (var versionDir in packageDir.GetDirectories())
            {
                if (NuGetVersion.TryParse(versionDir.Name, out var version) && (maxVersion == null || version > maxVersion))
                {
                    maxVersion = version;
                    maxVersionDir = versionDir;
                }
            }

            if (maxVersionDir == null || maxVersion == null)
                return;

            var nuspecFile = Path.Join(maxVersionDir.FullName, $"{packageDir.Name}.nuspec");
            if (!File.Exists(nuspecFile))
                return;

            using var fileStream = File.OpenRead(nuspecFile);
            using var reader = new IgnoreNamespaceXmlReader(fileStream);
            Nuspec? nuspec;
            try
            {
                nuspec = (Nuspec?)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to parse nuspec {File}: {Message}", nuspecFile, ex.Message);
                return;
            }

            if (nuspec == null)
            {
                Log.Warn("Failed to parse nuspec: {File}", nuspecFile);
                return;
            }

            if (nuspec.Metadata.PackageTypes?.Any(b => b.Name == "CringePlugin") != true)
                return;

            if (!string.IsNullOrEmpty(searchText) && !nuspec.Metadata.Id.Contains(searchText, StringComparison.OrdinalIgnoreCase) &&
                nuspec.Metadata.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) != true &&
                nuspec.Metadata.Authors?.Contains(searchText, StringComparison.OrdinalIgnoreCase) != true &&
                nuspec.Metadata.Description?.Contains(searchText, StringComparison.OrdinalIgnoreCase) != true)
            {
                return;
            }

            Interlocked.Increment(ref entries);
            var data = nuspec.Metadata;

            string? icon = null;

            if (!string.IsNullOrEmpty(data.Icon))
            {
                var iconPath = Path.Join(maxVersionDir.FullName, data.Icon);

                if (Path.GetFullPath(iconPath).StartsWith(maxVersionDir.FullName, StringComparison.Ordinal) && File.Exists(iconPath))
                    icon = iconPath;
            }

            bag.Add(new SearchResultEntry(
                data.Id,
                NuGetVersion.Parse(data.Version),
                data.Description,
                [],
                data.Authors == null ? null : new(data.Authors, []),
                icon,
                null,
                null,
                data.ProjectUrl,
                null,
                null,
                [],
                data.Title,
                null,
                [.. data.PackageTypes.Select(x => new PackageType(x.Name))]));
        }

        Parallel.ForEach(_cacheDir.GetDirectories(), LoadDir);

        return new SearchResult(entries, [.. bag]);
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
        Installed,
        Count
    }
}