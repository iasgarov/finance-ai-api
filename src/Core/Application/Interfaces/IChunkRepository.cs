using Application.Dtos;

namespace Application.Interfaces;

public interface IChunkRepository
{
    Task InsertAsync(string docId, string content, IReadOnlyList<float> vector);
    Task<IReadOnlyList<ChunkDto>> GetAllAsync();
    Task ClearAsync();
}
