namespace Application.Dtos;

public sealed class AskRequestDto
{
    public string Question { get; set; } = string.Empty;
    public int? TopK { get; set; }
    public string? ChatModel { get; set; }
    public string? EmbedModel { get; set; }
    public float? Temperature { get; set; }
    public string? SystemPrompt { get; set; }

}
