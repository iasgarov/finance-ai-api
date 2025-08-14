using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces;

namespace Application.Services;

public sealed class OllamaChatService: IChatService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _defaultModel;

    public OllamaChatService(IHttpClientFactory httpClientFactory, string defaultModel)
    {
        _httpClientFactory = httpClientFactory;
        _defaultModel = defaultModel;
    }

    public async Task<string> GenerateAsync(string prompt, float temperature = 0.2f, string? overrideModel = null, CancellationToken ct = default)
    {
        var http = _httpClientFactory.CreateClient("ollama");
        var model = overrideModel ?? _defaultModel;

        // 1) generate cəhdi
        var genPayload = new { model, prompt, stream = false, options = new { temperature } };
        using var genRes = await http.PostAsJsonAsync("/api/generate", genPayload, ct);
        var genRaw = await genRes.Content.ReadAsStringAsync(ct);

        if (genRes.IsSuccessStatusCode)
        {
            using var genDoc = JsonDocument.Parse(genRaw);
            return genDoc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }

        // 404 isə, chat endpoint-ə keç
        if (genRes.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            var chatPayload = new
            {
                model,
                messages = new[] { new { role = "user", content = prompt } },
                stream = false,
                options = new { temperature }
            };

            using var chatRes = await http.PostAsJsonAsync("/api/chat", chatPayload, ct);
            var chatRaw = await chatRes.Content.ReadAsStringAsync(ct);

            if (!chatRes.IsSuccessStatusCode)
                throw new HttpRequestException($"POST {http.BaseAddress}api/chat -> {(int)chatRes.StatusCode} {chatRes.ReasonPhrase}. Body: {chatRaw}");

            using var chatDoc = JsonDocument.Parse(chatRaw);
            // /api/chat cavabında mətn ya “message” içində, ya “response” sahəsində olur (versiyadan asılı)
            if (chatDoc.RootElement.TryGetProperty("message", out var msg)
                && msg.TryGetProperty("content", out var content))
                return content.GetString() ?? string.Empty;

            return chatDoc.RootElement.GetProperty("response").GetString() ?? string.Empty;
        }

        // başqa statuslarsa, detallı xəta at
        throw new HttpRequestException($"POST {http.BaseAddress}api/generate -> {(int)genRes.StatusCode} {genRes.ReasonPhrase}. Body: {genRaw}");
    }
}
