namespace Application.Interfaces;

public interface IEmbeddingService
{
     Task<IReadOnlyList<float>> EmbedAsync(string text, string? overrideModel = null, CancellationToken ct = default);
}
