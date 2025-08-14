namespace Application.Dtos;

public record RagOptionsDto(int TopK, int ChunkSizeChars, int ChunkOverlap);
