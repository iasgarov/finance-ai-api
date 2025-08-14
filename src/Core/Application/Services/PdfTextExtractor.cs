using System.Text;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;

namespace Application.Services;


public sealed class PdfTextExtractor : ITextExtractor
{
    public async Task<string> ExtractPlainTextAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file.Length == 0) return string.Empty;
        using var stream = file.OpenReadStream();
        using var doc = PdfDocument.Open(stream);
        var sb = new StringBuilder();
        foreach (var page in doc.GetPages())
        {
            ct.ThrowIfCancellationRequested();
            sb.AppendLine(page.Text);
        }
        return await Task.FromResult(sb.ToString());
    }
}
