using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces;

namespace Application.Services;

public sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _defaultModel;

    public OllamaEmbeddingService(IHttpClientFactory httpClientFactory, string defaultModel)
    {
        _httpClientFactory = httpClientFactory;
        _defaultModel = defaultModel;
    }
    public async Task<IReadOnlyList<float>> EmbedAsync(string text, string? overrideModel = null, CancellationToken ct = default)
    {
        var model = overrideModel ?? _defaultModel; // ya config-dən
        var http = _httpClientFactory.CreateClient("ollama"); // AD TAM EYNİDİR
        var payload = new { model, prompt = text }; // (Ollama üçün adətən "prompt")

        // Tam yol – başında '/' var
        using var res = await http.PostAsJsonAsync("/api/embeddings", payload, ct);
        var raw = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Embeddings call failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {raw}");

        using var doc = JsonDocument.Parse(raw);
        var arr = doc.RootElement.GetProperty("embedding").EnumerateArray().Select(e => (float)e.GetDouble()).ToArray();
        return arr;
    }

}
