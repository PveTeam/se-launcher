using System.Net.Http.Json;

namespace CringeLauncher.CrashPad;

internal class HastebinUploader(Uri baseUri)
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = baseUri
    };

    public async Task<string> UploadAsync(string content)
    {
        var response = await _httpClient.PostAsync("documents", new StringContent(content));

        // todo change this to gracefully handle server errors
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadFromJsonAsync<Response>();

        return new Uri(baseUri, responseContent!.Key).ToString();
    }

    private record Response(string Key);
}