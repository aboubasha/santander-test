using System.Net.Http.Json;
using TestApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

app.MapGet("/test", () => Results.Ok(new { result = "OK" }));

app.MapGet("/hn/best", async (IHttpClientFactory httpFactory, CancellationToken ct) =>
{
    var client = httpFactory.CreateClient();
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");

    var ids = await client.GetFromJsonAsync<int[]>("v0/beststories.json", cancellationToken: ct);
    if (ids == null || ids.Length == 0)
    {
        return Results.Ok(Array.Empty<object>());
    }

    var tasks = ids.Select(async id =>
    {
        try
        {
            var item = await client.GetFromJsonAsync<HnItem>($"v0/item/{id}.json", ct);
            return (object?)item;
        }
        catch
        {
            return null;
        }
    });

    var stories = await Task.WhenAll(tasks);
    var result = stories.Where(s => s != null).ToArray()!;
    return Results.Ok(result);
});

app.Run();
