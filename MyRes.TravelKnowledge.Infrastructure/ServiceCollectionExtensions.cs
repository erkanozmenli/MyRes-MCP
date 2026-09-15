using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyRes.TravelKnowledge.Application;
using MyRes.TravelKnowledge.Infrastructure.Qdrant;
using MyRes.TravelKnowledge.Infrastructure.Seeding;
using OpenAI.Embeddings;

namespace MyRes.TravelKnowledge.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddTravelKnowledgeInfrastructure(
        this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<TravelKnowledgeOptions>()
            .Bind(builder.Configuration.GetSection(TravelKnowledgeOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.CollectionName), "CollectionName is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.EmbeddingModel), "EmbeddingModel is required.")
            .Validate(options => options.VectorSize > 0, "VectorSize must be positive.")
            .ValidateOnStart();

        builder.AddQdrantClient("qdrant");
        builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(services =>
        {
            var options = services.GetRequiredService<IOptions<TravelKnowledgeOptions>>().Value;
            var apiKey = builder.Configuration["OPENAI_API_KEY"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OPENAI_API_KEY is not configured.");

            return new EmbeddingClient(options.EmbeddingModel, apiKey).AsIEmbeddingGenerator();
        });

        builder.Services.AddSingleton<ITravelKnowledgeRepository, QdrantTravelKnowledgeRepository>();
        builder.Services.AddSingleton<QdrantCollectionInitializer>();
        builder.Services.AddSingleton<TravelKnowledgeDocumentParser>();
        builder.Services.AddSingleton<TravelKnowledgeChunker>();
        builder.Services.AddSingleton<TravelKnowledgeSeeder>();
        return builder;
    }
}
