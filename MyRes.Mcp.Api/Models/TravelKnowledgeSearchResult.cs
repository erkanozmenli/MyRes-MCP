namespace MyRes.Mcp.Api.Models;

public sealed record TravelKnowledgeSearchResult(
    string Content,
    string Country,
    string Topic,
    string Source,
    string? SourceUrl,
    string DocumentTitle,
    string? Section,
    double Score);
