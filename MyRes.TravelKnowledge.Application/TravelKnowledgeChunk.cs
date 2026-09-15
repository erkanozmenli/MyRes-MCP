namespace MyRes.TravelKnowledge.Application;

public sealed record TravelKnowledgeChunk(
    Guid Id,
    string Content,
    string Country,
    string Topic,
    string Source,
    string? SourceUrl,
    string DocumentTitle,
    string? Section);
