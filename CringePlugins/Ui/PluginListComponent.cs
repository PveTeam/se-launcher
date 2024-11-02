using System.Collections.Immutable;
using System.Text.Json;
using CringePlugins.Abstractions;
using CringePlugins.Config;
using CringePlugins.Resolver;
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

public class PluginListComponent : IRenderComponent
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private ImmutableDictionary<string, VersionRange> _packages;
    private ImmutableDictionary<NuGetClient, SearchResult>? _searchResults;
    private string _searchQuery = "";
    private Task? _searchTask;

    private bool _changed;
    private bool _open = true;
    private readonly PackagesConfig _packagesConfig;
    private readonly PackageSourceMapping _sources;
    private readonly string _configPath;
    private (SearchResultEntry entry, NuGetClient client)? _selected;

    public PluginListComponent(PackagesConfig packagesConfig, PackageSourceMapping sources, string configPath)
    {
        _packagesConfig = packagesConfig;
        _sources = sources;
        _configPath = configPath;
        _packages = packagesConfig.Packages.ToImmutableDictionary(b => b.Id, b => b.Range, StringComparer.OrdinalIgnoreCase);

        MyGuiSandbox.GuiControlCreated += GuiControlCreated;
    }

    private void GuiControlCreated(object obj)
    {
        if (obj is MyGuiScreenMainMenu && MyGuiScreenGamePlay.Static is null)
            _open = true;
    }

    public void OnFrame()
    {
        if (!_open) return;
        
        SetNextWindowSize(new(700, 500), ImGuiCond.FirstUseEver);
        
        if (!Begin("Plugin List", ref _open))
        {
            End();
            return;
        }
        
        if (_changed)
            TextDisabled("Changes would be applied on the next restart");

        if (BeginTabBar("Main"))
        {
            // TODO support for opening plugin loader plugin config (reflection call to a specific method)
            if (BeginTabItem("Installed Plugins"))
            {
                if (BeginTable("InstalledTable", 2, ImGuiTableFlags.ScrollY))
                {
                    TableSetupColumn("Id");
                    TableSetupColumn("Version");
                    TableHeadersRow();
                        
                    foreach (var (id, versionRange) in _packages)
                    {
                        TableNextRow();
                            
                        TableNextColumn();
                        Text(id);
                        TableNextColumn();
                        Text(versionRange.MinVersion?.ToString() ?? versionRange.ToString());
                    }

                    EndTable();
                }
                
                EndTabItem();
            }

            if (BeginTabItem("Available Plugins"))
            {
                AvailablePluginsTab();

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
                
        InputText("", ref _searchQuery, 256);
        
        SameLine();

        if (Button("Search"))
        {
            _searchTask = RefreshAsync();
            return;
        }

        if (searchResults.IsEmpty || searchResults.Values.All(b => b.Entries.IsEmpty))
        {
            TextDisabled("Nothing found");
            return;
        }

        BeginChild("List", new(400, 0), ImGuiChildFlags.Border | ImGuiChildFlags.ResizeX);
        {
            if (BeginTable("AvailableTable", 3, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp))
            {
                TableSetupColumn("Id", ImGuiTableColumnFlags.None, .5f);
                TableSetupColumn("Version", ImGuiTableColumnFlags.None, .3f);
                TableSetupColumn("Installed", ImGuiTableColumnFlags.None, .2f);
                TableHeadersRow();

                foreach (var (client, result) in searchResults)
                {
                    foreach (var package in result.Entries.Take(100))
                    {
                        TableNextRow();
                            
                        TableNextColumn();

                        var selected = _selected?.entry.Id.Equals(package.Id, StringComparison.OrdinalIgnoreCase) == true;

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
                        BeginDisabled();
                        if (Checkbox("", ref installed))
                        {
                            _packages = installed
                                ? _packages.Add(package.Id, new(package.Version))
                                : _packages.Remove(package.Id);

                            Save();
                        }
                        EndDisabled();
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
                    TextLinkOpenURL(url, url);

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
        
        var builder = ImmutableDictionary.CreateBuilder<NuGetClient, SearchResult>();
        
        await foreach (var source in _sources)
        {
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

    private void Save()
    {
        _changed = true;
        
        using var stream = File.Create(_configPath);

        JsonSerializer.Serialize(stream, _packagesConfig with
        {
            Packages = [.._packages.Select(b => new PackageReference(b.Key, b.Value))]
        }, NuGetClient.SerializerOptions);
    }
}