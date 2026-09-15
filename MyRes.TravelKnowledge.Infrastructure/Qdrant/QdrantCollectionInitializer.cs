using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace MyRes.TravelKnowledge.Infrastructure.Qdrant;

public sealed class QdrantCollectionInitializer(
    QdrantClient client,
    IOptions<TravelKnowledgeOptions> options)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (await client.CollectionExistsAsync(settings.CollectionName, cancellationToken))
            return;

        await client.CreateCollectionAsync(
            settings.CollectionName,
            new VectorParams { Size = settings.VectorSize, Distance = Distance.Cosine },
            cancellationToken: cancellationToken);
    }
}
