using Microsoft.Extensions.Options;
using MyRes.TravelKnowledge.Application;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace MyRes.TravelKnowledge.Infrastructure.Qdrant;

public sealed class QdrantTravelKnowledgeRepository(
    QdrantClient client,
    IOptions<TravelKnowledgeOptions> options) : ITravelKnowledgeRepository
{
    private string CollectionName => options.Value.CollectionName;

    public async Task<bool> HasAnyAsync(CancellationToken cancellationToken = default) =>
        await client.CountAsync(CollectionName, cancellationToken: cancellationToken) > 0;

    public async Task UpsertAsync(
        IReadOnlyList<(TravelKnowledgeChunk Chunk, ReadOnlyMemory<float> Embedding)> items,
        CancellationToken cancellationToken = default)
    {
        var points = items.Select(item => new PointStruct
        {
            Id = new PointId { Uuid = item.Chunk.Id.ToString() },
            Vectors = item.Embedding.ToArray(),
            Payload =
            {
                ["content"] = item.Chunk.Content,
                ["country"] = item.Chunk.Country,
                ["topic"] = item.Chunk.Topic,
                ["source"] = item.Chunk.Source,
                ["sourceUrl"] = item.Chunk.SourceUrl ?? string.Empty,
                ["documentTitle"] = item.Chunk.DocumentTitle,
                ["section"] = item.Chunk.Section ?? string.Empty
            }
        }).ToList();

        await client.UpsertAsync(CollectionName, points, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<TravelKnowledgeSearchResult>> SearchAsync(
        ReadOnlyMemory<float> embedding,
        string? country,
        string? topic,
        int topK,
        CancellationToken cancellationToken = default)
    {
        Filter? filter = null;
        if (country is not null || topic is not null)
        {
            filter = new Filter();
            if (country is not null)
                filter.Must.Add(Match("country", country));
            if (topic is not null)
                filter.Must.Add(Match("topic", topic));
        }

        var points = await client.SearchAsync(
            CollectionName,
            embedding.ToArray(),
            filter: filter,
            limit: (ulong)topK,
            cancellationToken: cancellationToken);

        return points.Select(point => new TravelKnowledgeSearchResult(
            Text(point, "content"),
            Text(point, "country"),
            Text(point, "topic"),
            Text(point, "source"),
            NullableText(point, "sourceUrl"),
            Text(point, "documentTitle"),
            NullableText(point, "section"),
            point.Score)).ToList();
    }

    private static Condition Match(string key, string value) => new()
    {
        Field = new FieldCondition { Key = key, Match = new Match { Keyword = value } }
    };

    private static string Text(ScoredPoint point, string key) =>
        point.Payload.TryGetValue(key, out var value) ? value.StringValue : string.Empty;

    private static string? NullableText(ScoredPoint point, string key)
    {
        var value = Text(point, key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
