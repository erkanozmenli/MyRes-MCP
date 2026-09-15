using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using MyRes.ProviderService.Domain;
using MyRes.ProviderService.Infrastructure;

namespace MyRes.ProviderService.Api
{
    public sealed class HotelEndpoints : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/hotels");

            group.MapGet("/search", SearchHotels);
            group.MapPost("/details", GetHotelDetails);
        }

        private static Ok<IReadOnlyList<HotelSearchResult>> SearchHotels(
            string city,
            string? name,
            string? neighborhood,
            decimal? maxPricePerNight,
            decimal? minRating,
            int? starRating,
            bool? breakfastIncluded,
            bool? freeCancellation,
            string[]? amenities,
            double? maxDistanceToTimesSquareKm,
            InMemoryHotelRepository repository)
        {
            var hotels = repository.Search(
                city,
                name,
                neighborhood,
                maxPricePerNight,
                minRating,
                starRating,
                breakfastIncluded,
                freeCancellation,
                amenities,
                maxDistanceToTimesSquareKm);

            return TypedResults.Ok(hotels);
        }

        private static Ok<IReadOnlyList<Hotel>> GetHotelDetails(
            IReadOnlyList<Guid> hotelIds,
            InMemoryHotelRepository repository)
        {
            return TypedResults.Ok(repository.GetByIds(hotelIds));
        }
    }
}
