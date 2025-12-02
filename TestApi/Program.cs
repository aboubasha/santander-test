var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/test", () =>
{
    return Results.Ok(new { result = "OK" });
});

app.Run();
