using System.Text.Json;
using DocumentSearch.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using Nest;

namespace DocumentSearch.Api.Controllers;

[ApiController]
[Route("search")]
[EnableRateLimiting("tenant")]
public class SearchController : ControllerBase
{
    private readonly IElasticClient _elastic;
    private readonly IDistributedCache _cache;

    public SearchController(IElasticClient elastic, IDistributedCache cache)
    {
        _elastic = elastic;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromHeader(Name = "X-Tenant-Id")] string tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant))
            return BadRequest("X-Tenant-Id header is required");

        var cacheKey = $"search:{tenant}:{q}";
        var cached = await _cache.GetStringAsync(cacheKey);

        if (cached != null)
            return Ok(JsonSerializer.Deserialize<object>(cached));

        var response = await _elastic.SearchAsync<Document>(s => s
            .Query(qr => qr
                .Bool(b => b
                    .Must(
                        m => m.MultiMatch(mm => mm
                            .Fields(f => f
                                .Field(d => d.Title)
                                .Field(d => d.Content))
                            .Query(q)),
                        m => m.Term(t => t.Field("tenantId.keyword").Value(tenant))
                        //m => m.Term(t => t.Field(d => d.TenantId).Value(tenant))
                    )
                )
            )
        );

        var result = response.Documents;

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });

        return Ok(result);
    }
}
