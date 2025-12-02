using System.Net.Http.Json;
using TestApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<HnCacheService>();
var app = builder.Build();

app.MapGet("/test", () => Results.Ok(new { result = "OK" }));

app.MapGet("/hn/best", async (HnCacheService hnCache, HttpRequest req) =>
{
    // optional query param ?n=10 to return top N items (cache is already sorted)
    int? n = null;
    if (int.TryParse(req.Query["n"].FirstOrDefault(), out var parsed) && parsed > 0)
        n = parsed;

    // Start cache fetch that should continue caching even if the client disconnects
    var result = await hnCache.GetStories();
    if (n.HasValue)
    {
        var take = Math.Min(n.Value, result.Length);
        result = result.Take(take).ToArray();
    }

    return Results.Ok(result);
});

app.Run();
