using MyRes.ProviderService.Domain;

namespace MyRes.ProviderService.Infrastructure
{
    public sealed class InMemoryHotelRepository
    {
        private readonly List<Hotel> _hotels =
        [
            new(
                Guid.Parse("20000000-0000-0000-0000-000000000001"),
                "Bryant Courtyard Hotel",
                "New York",
                "Midtown",
                275.00m,
                8.7m,
                4,
                true,
                true,
                0.4,
                ["WiFi", "Fitness Center", "Restaurant"],
                "Upper-floor rooms facing the courtyard are generally quieter. The hotel does not operate an airport shuttle; private airport transportation can be arranged through the concierge for an additional fee."),

            new(
                Guid.Parse("20000000-0000-0000-0000-000000000002"),
                "Hudson Link Hotel",
                "New York",
                "Garment District",
                240.00m,
                8.3m,
                4,
                false,
                true,
                0.8,
                ["WiFi", "Fitness Center", "Bar"],
                "Complimentary shuttle service to and from JFK is available between 06:00 and 22:00 and must be reserved in advance. Street-facing rooms may experience traffic noise, particularly at night."),

            new(
                Guid.Parse("20000000-0000-0000-0000-000000000003"),
                "Library Square Suites",
                "New York",
                "Midtown",
                320.00m,
                9.1m,
                5,
                true,
                true,
                0.3,
                ["WiFi", "Business Center", "Fitness Center", "Restaurant"],
                "Rooms have enhanced sound insulation and the lobby includes several quiet work areas with power outlets. A small business lounge is available 24 hours a day for hotel guests. Airport transfers can be arranged for an additional fee."),

            new(
                Guid.Parse("20000000-0000-0000-0000-000000000004"),
                "West 36th Street Inn",
                "New York",
                "Hell's Kitchen",
                210.00m,
                8.0m,
                3,
                false,
                true,
                1.5,
                ["WiFi", "Laundry"],
                "Early check-in from 11:00 may be available for an additional fee, subject to room availability. Luggage can be stored free of charge before check-in and after checkout."),

            new(
                Guid.Parse("20000000-0000-0000-0000-000000000005"),
                "Broadway Lantern Hotel",
                "New York",
                "Theater District",
                290.00m,
                8.8m,
                4,
                true,
                false,
                0.7,
                ["WiFi", "Rooftop Bar", "Restaurant"],
                "The hotel is in a lively nightlife area and music from nearby venues may be audible in some rooms until late evening. The hotel does not provide an airport shuttle."),
        ];

        public IReadOnlyList<HotelSearchResult> Search(
            string city,
            string? name,
            string? neighborhood,
            decimal? maxPricePerNight,
            decimal? minRating,
            int? starRating,
            bool? breakfastIncluded,
            bool? freeCancellation,
            IReadOnlyList<string>? amenities,
            double? maxDistanceToTimesSquareKm)
        {
            var requestedAmenities = amenities?
                .Where(amenity => !string.IsNullOrWhiteSpace(amenity))
                .Select(amenity => amenity.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];

            return _hotels
                .Where(hotel => hotel.City.Equals(city.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(hotel => string.IsNullOrWhiteSpace(name) || hotel.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(hotel => string.IsNullOrWhiteSpace(neighborhood) || hotel.Neighborhood.Equals(neighborhood.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(hotel => maxPricePerNight is null || hotel.PricePerNight <= maxPricePerNight)
                .Where(hotel => minRating is null || hotel.Rating >= minRating)
                .Where(hotel => starRating is null || hotel.StarRating == starRating)
                .Where(hotel => breakfastIncluded is null || hotel.BreakfastIncluded == breakfastIncluded)
                .Where(hotel => freeCancellation is null || hotel.FreeCancellation == freeCancellation)
                .Where(hotel => requestedAmenities.All(requested =>
                    hotel.Amenities.Contains(requested, StringComparer.OrdinalIgnoreCase)))
                .Where(hotel => maxDistanceToTimesSquareKm is null || hotel.DistanceToTimesSquareKm <= maxDistanceToTimesSquareKm)
                .OrderBy(hotel => hotel.PricePerNight)
                .Select(ToSearchResult)
                .ToList();
        }

        public IReadOnlyList<Hotel> GetByIds(IReadOnlyCollection<Guid> ids)
        {
            var requestedIds = ids.ToHashSet();

            return _hotels
                .Where(hotel => requestedIds.Contains(hotel.Id))
                .ToList();
        }

        private static HotelSearchResult ToSearchResult(Hotel hotel) => new(
            hotel.Id,
            hotel.Name,
            hotel.City,
            hotel.Neighborhood,
            hotel.PricePerNight,
            hotel.Rating,
            hotel.StarRating,
            hotel.BreakfastIncluded,
            hotel.FreeCancellation,
            hotel.DistanceToTimesSquareKm,
            hotel.Amenities);
    }
}
