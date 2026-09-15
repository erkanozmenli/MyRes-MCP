using Microsoft.Extensions.AI;

namespace MyRes.TravelKnowledge.Application;

public sealed class TravelKnowledgeSearchService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    ITravelKnowledgeRepository repository)
{
    public async Task<IReadOnlyList<TravelKnowledgeSearchResult>> SearchAsync(
        TravelKnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Query);
        if (request.TopK is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(request.TopK), "TopK must be between 1 and 20.");

        var generated = await embeddingGenerator.GenerateAsync(
            [request.Query.Trim()], cancellationToken: cancellationToken);

        return await repository.SearchAsync(
            generated[0].Vector,
            NormalizeCountry(request.Country),
            NormalizeTopic(request.Topic),
            request.TopK ?? 5,
            cancellationToken);
    }

    private static string? NormalizeCountry(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string? NormalizeTopic(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
