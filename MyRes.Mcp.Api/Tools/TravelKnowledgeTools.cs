using ModelContextProtocol.Server;
using MyRes.Mcp.Api.Models;
using MyRes.Mcp.Api.Services;
using System.ComponentModel;

namespace MyRes.Mcp.Api.Tools;

[McpServerToolType]
public sealed class TravelKnowledgeTools(TravelKnowledgeApiClient client)
{
    [McpServerTool]
    [Description("Searches the travel knowledge base for visa, passport, entry, transit, immigration, and destination travel information. Returns ranked source chunks and does not generate a final answer.")]
    public Task<IReadOnlyList<TravelKnowledgeSearchResult>> SearchTravelKnowledge(
        [Description("The travel-information question or search query.")] string query,
        [Description("Optional ISO-style country code, for example US.")] string? country = null,
        [Description("Optional topic such as visa, passport, entry, transit, or immigration.")] string? topic = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("A search query is required.", nameof(query));
        //if (topK is < 1 or > 20)
        //    throw new ArgumentOutOfRangeException(nameof(topK), "TopK must be between 1 and 20.");

        return client.SearchAsync(query.Trim(), country, topic, topK: null, cancellationToken);
    }
}
