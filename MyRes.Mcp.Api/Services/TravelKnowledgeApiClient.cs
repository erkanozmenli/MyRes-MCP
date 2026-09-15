using MyRes.Mcp.Api.Models;

namespace MyRes.Mcp.Api.Services;

public sealed class TravelKnowledgeApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<TravelKnowledgeSearchResult>> SearchAsync(
        string query,
        string? country = null,
        string? topic = null,
        int? topK = null,
        CancellationToken cancellationToken = default)
    {
        var request = new { Query = query, Country = country, Topic = topic, TopK = topK };
        using var response = await httpClient.PostAsJsonAsync(
            "/travel-knowledge/search", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<TravelKnowledgeSearchResult>>(
            cancellationToken) ?? [];
    }
}
