namespace Application.Interfaces;

public interface IChatService
{
    Task<string> GenerateAsync(string prompt, float temperature = 0.2f, string? overrideModel = null, CancellationToken ct = default);
}

