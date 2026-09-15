using MyRes.Mcp.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddHttpClient<FlightApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://myres-providerservice-api");
    })
    .AddServiceDiscovery();

builder.Services
    .AddHttpClient<TravelKnowledgeApiClient>(client =>
    {
        client.BaseAddress = new Uri("http://myres-travelknowledge-api");
    })
    .AddServiceDiscovery();

builder.Services
    .AddHttpClient<HotelApiClient>(client =>
    {
        client.BaseAddress = new Uri("https://myres-providerservice-api");
    })
    .AddServiceDiscovery();

builder.Services.AddServiceDiscovery();

builder.AddRedisClient("redis");
builder.Services.AddSingleton<AirportSearchService>();

// MCP
builder.Services
        .AddMcpServer(options =>
        {
            options.ServerInstructions = "";
        })
        .WithHttpTransport()
        .WithToolsFromAssembly();

var app = builder.Build();

await app.Services
    .GetRequiredService<AirportSearchService>()
    .InitializeAsync();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILogger<Program>>();

    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    string? requestBody = null;

    if (context.Request.Method == HttpMethods.Post &&
        context.Request.Path.StartsWithSegments("/mcp"))
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        requestBody = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        if (requestBody.Contains("\"method\":\"initialize\"",
                StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("MCP INITIALIZE received.");
        }
    }

    logger.LogInformation(
        "HTTP Request: {Method} {Path} Body: {Body}",
        context.Request.Method,
        context.Request.Path,
        requestBody);

    await next();

    stopwatch.Stop();

    logger.LogInformation(
        "HTTP Response: {Method} {Path} => {StatusCode} ({ElapsedMs} ms)",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds);
});

app.MapMcp("/mcp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.Run();
