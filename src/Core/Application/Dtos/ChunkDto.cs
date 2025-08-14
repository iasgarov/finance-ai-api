namespace Application.Dtos;

public sealed class ChunkDto
{
    public long Id { get; set; }
    public string DocId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string VectorJson { get; set; } = string.Empty;
}
