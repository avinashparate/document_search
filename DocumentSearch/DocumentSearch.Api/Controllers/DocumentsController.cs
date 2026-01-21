using System.Text.Json;
using DocumentSearch.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using Nest;

namespace DocumentSearch.Api.Controllers;

[ApiController]
[Route("documents")]
[EnableRateLimiting("tenant")]
public class DocumentsController : ControllerBase
{
    private readonly IElasticClient _elastic;
    private readonly IDistributedCache _cache;

    public DocumentsController(IElasticClient elastic, IDistributedCache cache)
    {
        _elastic = elastic;
        _cache = cache;
    }

    [HttpPost]
    public async Task<IActionResult> Index(
        [FromHeader(Name = "X-Tenant-Id")] string tenant,
        [FromBody] Document document)
    {
        if (string.IsNullOrWhiteSpace(tenant))
            return BadRequest("X-Tenant-Id header is required");

        document.Id = Guid.NewGuid();
        document.TenantId = tenant;

        await _elastic.IndexDocumentAsync(document);
        await _cache.RemoveAsync($"search:{tenant}");

        return Ok(document);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(
    Guid id,
    [FromHeader(Name = "X-Tenant-Id")] string tenant)
    {
        var cacheKey = $"doc:{tenant}:{id}";

        // 1️⃣ Cache lookup
        var cached = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return Ok(JsonSerializer.Deserialize<Document>(cached));
        }

        // 2️⃣ Fetch from Elasticsearch
        var response = await _elastic.GetAsync<Document>(id, g => g.Index("documents"));

        if (!response.Found || response.Source?.TenantId != tenant)
            return NotFound();

        // 3️⃣ Cache ONLY the document
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(response.Source),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });

        return Ok(response.Source);
    }

    /*    [HttpGet("{id}")]
        public async Task<IActionResult> Get(
            Guid id,
            [FromHeader(Name = "X-Tenant-Id")] string tenant)
        {
            var response = await _elastic.GetAsync<Document>(id);

            if (!response.Found || response.Source?.TenantId != tenant)
                return NotFound();

            return Ok(response.Source);
        }*/

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _elastic.DeleteAsync<Document>(id);
        return NoContent();
    }
}
