using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Services
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Elasticsearch
builder.Services.AddSingleton<IElasticClient>(_ =>
{
    var settings = new ConnectionSettings(new Uri("http://elasticsearch:9200"))
        .DefaultIndex("documents");
    return new ElasticClient(settings);
});

// Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});

// Rate Limiting (per tenant)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("tenant", limiter =>
    {
        limiter.Window = TimeSpan.FromSeconds(1);
        limiter.PermitLimit = 100;
    });
});

var app = builder.Build();

// --------------------
// Middleware
// --------------------
app.UseSwagger();
app.UseSwaggerUI();

app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

// Health Check
app.MapGet("/health", () => Results.Ok(new
{
    status = "UP",
    dependencies = new
    {
        elasticsearch = "UP",
        redis = "UP"
    }
}));

app.Run();
