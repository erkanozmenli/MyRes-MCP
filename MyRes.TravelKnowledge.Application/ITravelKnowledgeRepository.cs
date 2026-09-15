namespace MyRes.TravelKnowledge.Application;

public interface ITravelKnowledgeRepository
{
    Task<IReadOnlyList<TravelKnowledgeSearchResult>> SearchAsync(
        ReadOnlyMemory<float> embedding,
        string? country,
        string? topic,
        int topK,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyAsync(CancellationToken cancellationToken = default);

    Task UpsertAsync(
        IReadOnlyList<(TravelKnowledgeChunk Chunk, ReadOnlyMemory<float> Embedding)> items,
        CancellationToken cancellationToken = default);
}
