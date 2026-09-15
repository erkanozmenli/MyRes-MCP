using MyRes.TravelKnowledge.Application;
using MyRes.TravelKnowledge.Infrastructure;
using MyRes.TravelKnowledge.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTravelKnowledgeApplication();
builder.AddTravelKnowledgeInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.MapPost("/travel-knowledge/search", async (
    TravelKnowledgeSearchRequest request,
    TravelKnowledgeSearchService service,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Query))
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.Query)] = ["Query is required."]
        });

    if (request.TopK is < 1 or > 20)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [nameof(request.TopK)] = ["TopK must be between 1 and 20."]
        });

    return Results.Ok(await service.SearchAsync(request, cancellationToken));
});

await app.Services.GetRequiredService<TravelKnowledgeSeeder>().SeedAsync();

app.Run();
