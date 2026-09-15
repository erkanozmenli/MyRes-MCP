namespace MyRes.TravelKnowledge.Application;

public sealed record TravelKnowledgeSearchRequest(
    string Query,
    string? Country = null,
    string? Topic = null,
    int? TopK = null);
