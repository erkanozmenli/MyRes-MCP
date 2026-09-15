using MyRes.Mcp.Api.Models;
using System.Globalization;

namespace MyRes.Mcp.Api.Services
{
    public sealed class FlightApiClient
    {
        private readonly HttpClient _httpClient;

        public FlightApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<Flight>> GetFlightsAsync(CancellationToken cancellationToken = default)
        {
            var flights = await _httpClient.GetFromJsonAsync<List<Flight>>("/api/flights", cancellationToken);

            return flights ?? [];
        }

        public async Task<IReadOnlyList<Flight>> SearchFlightsAsync(
            string? origin,
            string? destination,
            DateOnly? departureDate,
            string? cabinClass,
            decimal? maxPrice,
            int? maxStops,
            CancellationToken cancellationToken = default)
        {
            var query = new List<string>();

            if (!string.IsNullOrWhiteSpace(origin))
                query.Add($"origin={Uri.EscapeDataString(origin)}");

            if (!string.IsNullOrWhiteSpace(destination))
                query.Add($"destination={Uri.EscapeDataString(destination)}");

            if (departureDate.HasValue)
                query.Add($"departureDate={departureDate.Value:yyyy-MM-dd}");

            if (!string.IsNullOrWhiteSpace(cabinClass))
                query.Add($"cabinClass={Uri.EscapeDataString(cabinClass)}");

            if (maxPrice.HasValue)
                query.Add($"maxPrice={maxPrice.Value.ToString(CultureInfo.InvariantCulture)}");

            if (maxStops.HasValue)
                query.Add($"maxStops={maxStops.Value}");

            var url = "/api/flights/search";

            if (query.Count > 0)
                url += "?" + string.Join("&", query);

            var flights = await _httpClient.GetFromJsonAsync<List<Flight>>(url, cancellationToken);

            return flights ?? [];
        }


    }
}
