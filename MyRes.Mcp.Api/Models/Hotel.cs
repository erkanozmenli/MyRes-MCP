namespace MyRes.Mcp.Api.Models
{
    public sealed record Hotel(
        Guid Id,
        string Name,
        string City,
        string Neighborhood,
        decimal PricePerNight,
        decimal Rating,
        int StarRating,
        bool BreakfastIncluded,
        bool FreeCancellation,
        double DistanceToTimesSquareKm,
        IReadOnlyList<string> Amenities,
        string Description);
}
