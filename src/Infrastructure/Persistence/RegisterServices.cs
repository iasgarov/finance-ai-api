using Application.Dtos;
using Application.Interfaces;
using Application.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Persistence.Repositories;
namespace Persistence;

public static class RegisterServices
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("RagDb")!;

        // Register connection string so repositories can use it
        services.AddSingleton(cs);

        // Ensure pgvector schema exists (run once at startup)
            using (var conn = new NpgsqlConnection(cs))
            {
                conn.Open();
                const string sql = """
                CREATE EXTENSION IF NOT EXISTS vector;
                CREATE TABLE IF NOT EXISTS chunks (
                id BIGSERIAL PRIMARY KEY,
                doc_id TEXT NOT NULL,
                content TEXT NOT NULL,
                embedding vector NOT NULL,
                vector_json  TEXT NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT now()
                );
            """;
                conn.Execute(sql);
            }

        // Register type handler for pgvector
        // SqlMapper.AddTypeHandler(new VectorTypeHandler());

        // Register repositories
        services.AddSingleton<IChunkRepository>(sp =>
            new ChunkRepository(cs));

        // Config
        var ollamaEndpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT")
                             ?? configuration.GetValue<string>("Ollama:Endpoint")
                             ?? "http://localhost:11434/";

        var chatModel = configuration.GetValue<string>("Ollama:ChatModel") ?? "llama3";
        var embedModel = configuration.GetValue<string>("Ollama:EmbedModel") ?? "nomic-embed-text";

        var topK = configuration.GetValue<int?>("Rag:TopK") ?? 5;
        var chunkSize = configuration.GetValue<int?>("Rag:ChunkSizeChars") ?? 1200;
        var chunkOverlap = configuration.GetValue<int?>("Rag:ChunkOverlap") ?? 200;

        services.AddSingleton<IEmbeddingService>(sp =>
        new OllamaEmbeddingService(sp.GetRequiredService<IHttpClientFactory>(), embedModel));
        services.AddSingleton<IChatService>(sp =>
            new OllamaChatService(sp.GetRequiredService<IHttpClientFactory>(), chatModel));
        services.AddSingleton<ITextExtractor, PdfTextExtractor>();
        services.AddSingleton<ITextSplitter>(_ => new TextSplitter(chunkSize, chunkOverlap));
        services.AddSingleton<IChunkRepository, ChunkRepository>();
        services.AddSingleton<VectorUtils>();
        services.AddSingleton(new RagOptionsDto(topK, chunkSize, chunkOverlap));
        return services;
    }
}
