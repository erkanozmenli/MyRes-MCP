using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using MyRes.ProviderService.Domain;
using MyRes.ProviderService.Infrastructure;

namespace MyRes.ProviderService.Api
{
    public sealed class FlightEndpoints(InMemoryFlightRepository repository) : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/flights");

            group.MapGet("/", GetFlights);
            group.MapGet("/{id:guid}", GetFlight);
            group.MapGet("/search", SearchFlights);
        }

        private static Ok<IReadOnlyList<Flight>> GetFlights(InMemoryFlightRepository repository)
        {
            return TypedResults.Ok(repository.GetAll());
        }

        private static Results<Ok<Flight>, NotFound> GetFlight(Guid id, InMemoryFlightRepository repository)
        {
            var flight = repository.GetById(id);

            return flight is null ? TypedResults.NotFound() : TypedResults.Ok(flight);
        }

        private static Ok<IReadOnlyList<Flight>> SearchFlights(
            string? origin,
            string? destination,
            DateOnly? departureDate,
            string? cabinClass,
            decimal? maxPrice,
            int? maxStops,
            InMemoryFlightRepository repository)
        {
            var flights = repository.Search(
                origin,
                destination,
                departureDate,
                cabinClass,
                maxPrice,
                maxStops);

            return TypedResults.Ok(flights);
        }
    }
}
