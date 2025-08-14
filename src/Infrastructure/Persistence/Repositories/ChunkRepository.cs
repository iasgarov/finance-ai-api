using System.Data;
using System.Text.Json;
using Application.Dtos;
using Application.Interfaces;
using Dapper;
using Npgsql;
using Pgvector;

namespace Persistence.Repositories;

public sealed class ChunkRepository : IChunkRepository
{
    private readonly string connectionString;

    public ChunkRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task InsertAsync(string docId, string content, IReadOnlyList<float> vector)
    {
        await using var db = new NpgsqlConnection(this.connectionString);
        await db.OpenAsync();
        var vec = new Vector(vector.ToArray());
        const string sql = "INSERT INTO chunks(doc_id, content, embedding,vector_json) VALUES (@doc, @content, @emb, @vectorJson)";
        var vecJson = JsonSerializer.Serialize(vector);
        await db.ExecuteAsync(sql, new { doc = docId, content, emb = vector.ToArray(), vectorJson= vecJson });
    }

    public async Task<IReadOnlyList<ChunkDto>> GetAllAsync()
    {
        await using var db = new NpgsqlConnection(this.connectionString);
        await db.OpenAsync();
        var rows = await db.QueryAsync<ChunkDto>("SELECT id, doc_id as DocId, content, vector_json as VectorJson FROM chunks");
        return rows.ToList();
    }

    public async Task ClearAsync()
    {
        await using var db = new NpgsqlConnection(this.connectionString);
        await db.OpenAsync();
        await db.ExecuteAsync("DELETE FROM chunks");
    }
}
