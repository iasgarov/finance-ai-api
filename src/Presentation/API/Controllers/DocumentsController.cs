using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ITextExtractor extractor;
    private readonly ITextSplitter splitter;
    private readonly IEmbeddingService embeddingService;
    private readonly IChunkRepository chunkRepository;

    public DocumentsController(ITextExtractor extractor, ITextSplitter splitter, IEmbeddingService embeddingService, IChunkRepository chunkRepository)
    {
        this.extractor = extractor;
        this.splitter = splitter;
        this.embeddingService = embeddingService;
        this.chunkRepository = chunkRepository;
    }
    
     [HttpPost("upload")]
    [RequestSizeLimit(1024L * 1024L * 100L)] // 100MB
    [ProducesResponseType(typeof(UploadResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UploadResponseDto>> Upload(IFormFile pdf, CancellationToken ct)
    {
        if (pdf == null || pdf.Length == 0) return BadRequest("Provide 'pdf' file via multipart/form-data.");

        var text = await this.extractor.ExtractPlainTextAsync(pdf, ct);
        var chunks = this.splitter.Split(text);
        var docId = Guid.NewGuid().ToString("N");

        foreach (var chunk in chunks)
        {
            var vec = await this.embeddingService.EmbedAsync(chunk, null, ct);
            await this.chunkRepository.InsertAsync(docId, chunk, vec);
        }

        return Ok(new UploadResponseDto { DocId = docId, Chunks = chunks.Count });
    }

}
