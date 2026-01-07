using System.Net;
using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using NLog;
using Pillar.Demystifier;

namespace CringeLauncher.CrashPad;

internal class PortableDebugSymbolsResolver : IPortableDebugSymbolsResolver
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly HttpClient _client = new()
    {
        BaseAddress = new("https://ng.zznty.ru/api/download/symbols/")
    };
    private readonly string _cacheDir = Directory.CreateDirectory(Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CringeLauncher", "cache", "pdb")).FullName;
    public async ValueTask<string?> ResolvePdbFileAsync(Module module, Guid pdbId, string pdbPath)
    {
        var loadContext = AssemblyLoadContext.GetLoadContext(module.Assembly);

        var candidate = await IPortableDebugSymbolsResolver.Default.ResolvePdbFileAsync(module, pdbId, pdbPath);
        if (candidate is not null || loadContext is not ICoreLoadContext)
            return candidate;

        var fileName = Path.GetFileName(pdbPath);
        var id = pdbId.ToString("N");

        var filePath = Path.Join(_cacheDir, id);

        if (File.Exists(filePath))
            return filePath;
        
        try
        {
            await using var stream = await _client.GetStreamAsync($"{fileName}/{id}FFFFFFFF/{fileName}");
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);

            return filePath;
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode is HttpStatusCode.NotFound or >= HttpStatusCode.InternalServerError)
        {
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to download {FileName} {id} for {ModuleName}", fileName, id, module.Name);
            return null;
        }
    }
}