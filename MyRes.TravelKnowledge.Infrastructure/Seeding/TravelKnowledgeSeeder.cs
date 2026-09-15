using System.Reflection;
using Microsoft.Extensions.AI;
using MyRes.TravelKnowledge.Application;
using MyRes.TravelKnowledge.Infrastructure.Qdrant;

namespace MyRes.TravelKnowledge.Infrastructure.Seeding;

public sealed class TravelKnowledgeSeeder(
    QdrantCollectionInitializer initializer,
    ITravelKnowledgeRepository repository,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    TravelKnowledgeDocumentParser parser,
    TravelKnowledgeChunker chunker)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await initializer.InitializeAsync(cancellationToken);
        if (await repository.HasAnyAsync(cancellationToken))
            return;

        var assembly = typeof(TravelKnowledgeSeeder).Assembly;
        var chunks = new List<TravelKnowledgeChunk>();
        foreach (var resource in assembly.GetManifestResourceNames()
                     .Where(name => name.Contains(".Seeding.Data.") && name.EndsWith(".md"))
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            await using var stream = assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Could not read embedded seed '{resource}'.");
            using var reader = new StreamReader(stream);
            chunks.AddRange(chunker.Chunk(parser.Parse(await reader.ReadToEndAsync(cancellationToken))));
        }

        var generated = await embeddingGenerator.GenerateAsync(
            chunks.Select(chunk => chunk.Content), cancellationToken: cancellationToken);
        var items = chunks.Select((chunk, index) =>
            (chunk, (ReadOnlyMemory<float>)generated[index].Vector)).ToList();
        await repository.UpsertAsync(items, cancellationToken);
    }
}
