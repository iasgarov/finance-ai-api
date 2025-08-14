using Application.Interfaces;

namespace Application.Services;


public sealed class TextSplitter : ITextSplitter
{
    private readonly int _chunk;
    private readonly int _overlap;

    public TextSplitter(int chunkSize, int overlap)
    {
        _chunk = Math.Max(200, chunkSize);
        _overlap = Math.Max(0, overlap);
    }

    public IReadOnlyList<string> Split(string text)
    {
        var clean = text.Replace("\r", "");
        var chunks = new List<string>();
        var start = 0;
        var len = clean.Length;
        while (start < len)
        {
            var end = Math.Min(start + _chunk, len);
            var piece = clean[start..end];
            var trimmed = piece.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed)) chunks.Add(trimmed);
            if (end == len) break;
            start = Math.Max(end - _overlap, 0);
            if (start >= len) break;
        }
        return chunks;
    }
}
