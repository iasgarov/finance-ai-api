using Application.Dtos;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AskController : ControllerBase
{
    private readonly IChunkRepository chunkRepository;
    private readonly IEmbeddingService embeddingService;
    private readonly IChatService chatService;
    private readonly RagOptionsDto ragOptions;

    public AskController(IChunkRepository chunkRepository, IEmbeddingService embeddingService, IChatService chatService, RagOptionsDto ragOptions)
    {
        this.chunkRepository = chunkRepository;
        this.embeddingService = embeddingService;
        this.chatService = chatService;
        this.ragOptions = ragOptions;
    }

    [HttpPost]
    public async Task<ActionResult<AskResponseDto>> Ask([FromBody] AskRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var rows = await this.chunkRepository.GetAllAsync();
        if (rows.Count == 0)
            return BadRequest("No documents indexed. Upload a PDF first at /api/documents/upload.");

        var qVec = await this.embeddingService.EmbedAsync(request.Question, request.EmbedModel, ct);

        var scored = rows
            .Select(r => new
            {
                Row = r,
                Score = VectorUtils.CosineSimilarity(qVec, VectorUtils.ParseJsonVector(r.VectorJson))
            })
            .OrderByDescending(x => x.Score)
            .Take(request.TopK ?? this.ragOptions.TopK)
            .ToList();

        var context = string.Join("\n---\n", scored.Select(s => s.Row.Content));

        var systemPrompt = request.SystemPrompt ?? "You are a helpful assistant. Use the provided context to answer. If the answer isn’t in the context, say you don’t know.";

        var finalPrompt =
            "SYSTEM:\n" + systemPrompt + "\n\n" +
            "CONTEXT (from PDFs):\n" + context + "\n\n" +
            "USER QUESTION:\n" + request.Question + "\n";

        var temperature = request.Temperature ?? 0.2f;
        var answer = await this.chatService.GenerateAsync(finalPrompt, temperature, request.ChatModel, ct);

        var resp = new AskResponseDto
        {
            Answer = answer,
            Model = request.ChatModel ?? "llama3",
            UsedChunks = scored.Select(s => new { s.Score, preview = s.Row.Content.Length > 280 ? s.Row.Content[..280] + "…" : s.Row.Content })
        };

        return Ok(resp);
    }

}
