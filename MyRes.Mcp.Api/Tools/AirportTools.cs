using ModelContextProtocol.Server;
using MyRes.Mcp.Api.Models;
using MyRes.Mcp.Api.Services;
using System.ComponentModel;

namespace MyRes.Mcp.Api.Tools
{
    [McpServerToolType]
    public sealed class AirportTools(AirportSearchService airportSearchService)
    {
        [McpServerTool]
        [Description(
            "Searches airports by city name, airport name, or IATA airport code, including spelling variations. " +
            "Returns canonical airport candidates that can be used with search_flights.")]
        public Task<IReadOnlyList<Airport>> SearchAirports(
            [Description(
                "City name, airport name, or IATA airport code, for example Istanbul, " +
                "Istanbul Airport, New York, IST, or JFK.")]
            string query,
            CancellationToken cancellationToken = default)
        {
            return airportSearchService.SearchAsync(query, cancellationToken);
        }
    }
}
