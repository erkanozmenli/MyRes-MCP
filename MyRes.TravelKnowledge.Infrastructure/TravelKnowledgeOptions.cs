namespace MyRes.TravelKnowledge.Infrastructure;

public sealed class TravelKnowledgeOptions
{
    public const string SectionName = "TravelKnowledge";
    public string CollectionName { get; set; } = "travel-knowledge";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public ulong VectorSize { get; set; } = 1536;
}
