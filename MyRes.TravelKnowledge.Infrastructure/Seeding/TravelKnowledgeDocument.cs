namespace MyRes.TravelKnowledge.Infrastructure.Seeding;

internal sealed record TravelKnowledgeDocument(
    string Title,
    string Country,
    string Topic,
    string Source,
    string? SourceUrl,
    string Body);
