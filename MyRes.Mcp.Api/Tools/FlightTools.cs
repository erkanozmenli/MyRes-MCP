using ModelContextProtocol.Server;
using MyRes.Mcp.Api.Services;
using System.ComponentModel;

namespace MyRes.Mcp.Api.Tools
{
    [McpServerToolType]
    public sealed class FlightTools
    {
        private readonly FlightApiClient _flightApiClient;

        public FlightTools(FlightApiClient flightApiClient)
        {
            _flightApiClient = flightApiClient;
        }

        [McpServerTool]
        [Description("Lists all available flights from the flight provider.")]
        public async Task<IReadOnlyList<Models.Flight>> GetFlights(CancellationToken cancellationToken = default)
        {
            return await _flightApiClient.GetFlightsAsync(cancellationToken);
        }

        [McpServerTool]
        [Description(
            "Searches available flights using departure and arrival IATA airport codes and " +
            "departure date. Optional search filters such as cabin class, maximum price, and " +
            "maximum stops must only be included when explicitly specified by the user.")]
        public async Task<IReadOnlyList<Models.Flight>> SearchFlights(
        [Description(
            "Departure IATA airport codes. Use codes returned by search_airports. " +
            "For example: IST or SAW.")]
        IReadOnlyList<string>? origins = null,

        [Description(
            "Arrival IATA airport codes. Use codes returned by search_airports. " +
            "For example: JFK, EWR, or LGA.")]
        IReadOnlyList<string>? destinations = null,

        [Description(
            "Departure date for the flight search. Convert dates provided by the user " +
            "into yyyy-MM-dd format when calling this tool. Do not ask the user to " +
            "provide or confirm this technical format.")]
        DateOnly? departureDate = null,

        [Description(
            "OPTIONAL. Cabin class. ONLY include this parameter when the user explicitly " +
            "specifies a cabin class. NEVER assume or infer a cabin class such as Economy.")]
        string? cabinClass = null,

        [Description(
            "OPTIONAL. Maximum ticket price. ONLY include this parameter when the user " +
            "explicitly specifies a maximum price. NEVER assume, infer, or invent a price.")]
        decimal? maxPrice = null,

        [Description(
            "OPTIONAL. Maximum number of stops. ONLY include this parameter when the user " +
            "explicitly specifies a stop limit. NEVER assume nonstop or any other value.")]
        int? maxStops = null,

        CancellationToken cancellationToken = default)
        {
            var originCodes = ValidateIataCodes(origins, nameof(origins));
            var destinationCodes = ValidateIataCodes(destinations, nameof(destinations));

            if (originCodes.Count == 0 || destinationCodes.Count == 0)
                return [];


            var searches =
                from originCode in originCodes
                from destinationCode in destinationCodes
                select _flightApiClient.SearchFlightsAsync(
                    originCode,
                    destinationCode,
                    departureDate,
                    cabinClass,
                    maxPrice,
                    maxStops,
                    cancellationToken);

            var results = await Task.WhenAll(searches);

            return results
                .SelectMany(flights => flights)
                .DistinctBy(flight => flight.Id)
                .OrderBy(flight => flight.Price)
                .ToList();
        }

        private static IReadOnlyList<string?> ValidateIataCodes(
            IReadOnlyList<string>? codes,
            string parameterName)
        {
            if (codes is null)
                return [null];

            var normalizedCodes = new List<string?>(codes.Count);

            foreach (var code in codes)
            {
                var normalizedCode = code?.Trim().ToUpperInvariant();

                if (normalizedCode is null ||
                    normalizedCode.Length != 3 ||
                    normalizedCode.Any(character => character is < 'A' or > 'Z'))
                    throw new ArgumentException(
                        $"'{code}' is not a valid IATA airport code. " +
                        "Use codes returned by search_airports.",
                        parameterName);

                normalizedCodes.Add(normalizedCode);
            }

            return normalizedCodes.Distinct(StringComparer.Ordinal).ToList();
        }
    }
}
