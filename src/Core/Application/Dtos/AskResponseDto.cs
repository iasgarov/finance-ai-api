namespace Application.Dtos;

public sealed class AskResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public IEnumerable<object> UsedChunks { get; set; } = Array.Empty<object>();
}
