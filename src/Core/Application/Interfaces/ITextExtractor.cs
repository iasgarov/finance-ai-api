using Microsoft.AspNetCore.Http;
namespace Application.Interfaces;

public interface ITextExtractor
{
    Task<string> ExtractPlainTextAsync(IFormFile file, CancellationToken ct = default);
}
