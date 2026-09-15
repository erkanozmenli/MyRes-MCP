using ModelContextProtocol.Server;
using MyRes.Mcp.Api.Services;
using System.ComponentModel;

namespace MyRes.Mcp.Api.Tools
{
    [McpServerToolType]
    public sealed class HotelTools(HotelApiClient hotelApiClient)
    {
        [McpServerTool]
        [Description(
            "Searches hotels using structured hotel attributes only. Does not search, inspect, " +
            "or interpret hotel descriptions. Use get_hotel_details when the user asks about " +
            "quietness, noise, airport transportation, early check-in, luggage storage, or work suitability.")]
        public async Task<IReadOnlyList<Models.HotelSearchResult>> SearchHotels(
            [Description("Required city in which to search for hotels, for example New York.")]
            string city,
            [Description("OPTIONAL. Full or partial hotel name, used to resolve a hotel before requesting details.")]
            string? name = null,
            [Description("OPTIONAL. Hotel neighborhood.")]
            string? neighborhood = null,
            [Description("OPTIONAL. Maximum nightly price. Only include when specified by the user.")]
            decimal? maxPricePerNight = null,
            [Description("OPTIONAL. Minimum guest rating. Only include when specified by the user.")]
            decimal? minRating = null,
            [Description("OPTIONAL. Exact hotel star rating. Only include when specified by the user.")]
            int? starRating = null,
            [Description("OPTIONAL. Whether breakfast must be included. Only include when specified by the user.")]
            bool? breakfastIncluded = null,
            [Description("OPTIONAL. Whether free cancellation must be offered. Only include when specified by the user.")]
            bool? freeCancellation = null,
            [Description("OPTIONAL. Amenities that must all be present. Do not use this for description-only facts.")]
            IReadOnlyList<string>? amenities = null,
            [Description("OPTIONAL. Maximum distance from Times Square in kilometers. Only include when specified by the user.")]
            double? maxDistanceToTimesSquareKm = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required.", nameof(city));

            return await hotelApiClient.SearchHotelsAsync(
                city.Trim(),
                name,
                neighborhood,
                maxPricePerNight,
                minRating,
                starRating,
                breakfastIncluded,
                freeCancellation,
                amenities,
                maxDistanceToTimesSquareKm,
                cancellationToken);
        }

        [McpServerTool]
        [Description(
            "Returns detailed information for one or more hotels, including their free-text descriptions. " +
            "Use this when information is needed that is not represented by the structured hotel search fields. " +
            "Pass all relevant hotel IDs in a single call rather than calling this tool separately for each hotel.")]
        public async Task<IReadOnlyList<Models.Hotel>> GetHotelDetails(
            [Description("One or more hotel IDs returned by search_hotels. Pass all relevant IDs in this single collection.")]
            IReadOnlyList<Guid> hotelIds,
            CancellationToken cancellationToken = default)
        {
            if (hotelIds is null)
                throw new ArgumentNullException(nameof(hotelIds));

            if (hotelIds.Count == 0)
                return [];

            return await hotelApiClient.GetHotelDetailsAsync(
                hotelIds.Distinct().ToList(),
                cancellationToken);
        }
    }
}
