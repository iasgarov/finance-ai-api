namespace Application.Dtos;

public sealed class UploadResponseDto
{
    public string DocId { get; set; } = string.Empty;
    public int Chunks { get; set; }
}
