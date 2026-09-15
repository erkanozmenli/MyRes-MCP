using Microsoft.Extensions.DependencyInjection;

namespace MyRes.TravelKnowledge.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTravelKnowledgeApplication(this IServiceCollection services) =>
        services.AddScoped<TravelKnowledgeSearchService>();
}
