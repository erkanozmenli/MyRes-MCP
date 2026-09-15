namespace MyRes.ProviderService.Domain
{
    public sealed record Flight(
    Guid Id,
    string Airline,
    string FlightNumber,
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    TimeOnly DepartureTime,
    TimeOnly ArrivalTime,
    int DurationMinutes,
    decimal Price,
    string Currency,
    string CabinClass,
    int Stops,
    bool Available);
}
