using MyRes.Mcp.Api.Models;
using System.Globalization;
using System.Net.Http.Json;

namespace MyRes.Mcp.Api.Services
{
    public sealed class HotelApiClient(HttpClient httpClient)
    {
        public async Task<IReadOnlyList<HotelSearchResult>> SearchHotelsAsync(
            string city,
            string? name,
            string? neighborhood,
            decimal? maxPricePerNight,
            decimal? minRating,
            int? starRating,
            bool? breakfastIncluded,
            bool? freeCancellation,
            IReadOnlyList<string>? amenities,
            double? maxDistanceToTimesSquareKm,
            CancellationToken cancellationToken = default)
        {
            var query = new List<string>
            {
                $"city={Uri.EscapeDataString(city)}"
            };

            AddString(query, "name", name);
            AddString(query, "neighborhood", neighborhood);

            if (maxPricePerNight.HasValue)
                query.Add($"maxPricePerNight={maxPricePerNight.Value.ToString(CultureInfo.InvariantCulture)}");

            if (minRating.HasValue)
                query.Add($"minRating={minRating.Value.ToString(CultureInfo.InvariantCulture)}");

            if (starRating.HasValue)
                query.Add($"starRating={starRating.Value}");

            if (breakfastIncluded.HasValue)
                query.Add($"breakfastIncluded={breakfastIncluded.Value.ToString().ToLowerInvariant()}");

            if (freeCancellation.HasValue)
                query.Add($"freeCancellation={freeCancellation.Value.ToString().ToLowerInvariant()}");

            if (amenities is not null)
            {
                foreach (var amenity in amenities.Where(value => !string.IsNullOrWhiteSpace(value)))
                    query.Add($"amenities={Uri.EscapeDataString(amenity)}");
            }

            if (maxDistanceToTimesSquareKm.HasValue)
                query.Add($"maxDistanceToTimesSquareKm={maxDistanceToTimesSquareKm.Value.ToString(CultureInfo.InvariantCulture)}");

            var hotels = await httpClient.GetFromJsonAsync<List<HotelSearchResult>>(
                "/api/hotels/search?" + string.Join("&", query),
                cancellationToken);

            return hotels ?? [];
        }

        public async Task<IReadOnlyList<Hotel>> GetHotelDetailsAsync(
            IReadOnlyList<Guid> hotelIds,
            CancellationToken cancellationToken = default)
        {
            using var response = await httpClient.PostAsJsonAsync(
                "/api/hotels/details",
                hotelIds,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Hotel>>(cancellationToken) ?? [];
        }

        private static void AddString(List<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                query.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}
