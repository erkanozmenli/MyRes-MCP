using MyRes.ProviderService.Domain;

namespace MyRes.ProviderService.Infrastructure
{
    public sealed class InMemoryFlightRepository
    {
        private readonly List<Flight> _flights =
        [
             // Istanbul → London
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                "Turkish Airlines",
                "TK1983",
                "IST",
                "LHR",
                new DateOnly(2030, 9, 10),
                new TimeOnly(08, 15),
                new TimeOnly(10, 25),
                250,
                185.00m,
                "EUR",
                "Economy",
                0,
                true),

            new(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                "Turkish Airlines",
                "TK1985",
                "IST",
                "LHR",
                new DateOnly(2030, 9, 10),
                new TimeOnly(14, 30),
                new TimeOnly(16, 40),
                250,
                620.00m,
                "EUR",
                "Business",
                0,
                true),

            new(
                Guid.Parse("10000000-0000-0000-0000-000000000003"),
                "Lufthansa",
                "LH1299",
                "IST",
                "LHR",
                new DateOnly(2030, 9, 10),
                new TimeOnly(10, 45),
                new TimeOnly(15, 20),
                335,
                145.00m,
                "EUR",
                "Economy",
                1,
                true),

            // Istanbul → Paris
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000004"),
                "Turkish Airlines",
                "TK1821",
                "IST",
                "CDG",
                new DateOnly(2030, 9, 12),
                new TimeOnly(08, 00),
                new TimeOnly(10, 25),
                205,
                165.00m,
                "EUR",
                "Economy",
                0,
                true),

            new(
                Guid.Parse("10000000-0000-0000-0000-000000000005"),
                "Air France",
                "AF1391",
                "IST",
                "CDG",
                new DateOnly(2030, 9, 12),
                new TimeOnly(12, 10),
                new TimeOnly(14, 35),
                205,
                540.00m,
                "EUR",
                "Business",
                0,
                true),

            // Unavailable flight - should never appear in Search()
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000006"),
                "Pegasus",
                "PC1135",
                "IST",
                "CDG",
                new DateOnly(2030, 9, 12),
                new TimeOnly(16, 20),
                new TimeOnly(20, 50),
                270,
                120.00m,
                "EUR",
                "Economy",
                1,
                false),

            // Istanbul → New York
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000007"),
                "Turkish Airlines",
                "TK3",
                "IST",
                "JFK",
                new DateOnly(2030, 9, 15),
                new TimeOnly(07, 30),
                new TimeOnly(11, 20),
                650,
                520.00m,
                "USD",
                "Economy",
                0,
                true),

            new(
                Guid.Parse("10000000-0000-0000-0000-000000000008"),
                "LOT Polish Airlines",
                "LO138",
                "IST",
                "JFK",
                new DateOnly(2030, 9, 15),
                new TimeOnly(06, 50),
                new TimeOnly(15, 45),
                775,
                410.00m,
                "USD",
                "Economy",
                1,
                true),

            new(
                Guid.Parse("10000000-0000-0000-0000-000000000009"),
                "Turkish Airlines",
                "TK11",
                "IST",
                "JFK",
                new DateOnly(2030, 9, 15),
                new TimeOnly(13, 00),
                new TimeOnly(16, 50),
                650,
                1850.00m,
                "USD",
                "Business",
                0,
                true),

            // London → Istanbul
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000010"),
                "Turkish Airlines",
                "TK1984",
                "LHR",
                "IST",
                new DateOnly(2030, 9, 20),
                new TimeOnly(11, 30),
                new TimeOnly(17, 20),
                230,
                210.00m,
                "EUR",
                "Economy",
                0,
                true),

            // Paris → Istanbul
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000011"),
                "Air France",
                "AF1390",
                "CDG",
                "IST",
                new DateOnly(2030, 9, 22),
                new TimeOnly(09, 15),
                new TimeOnly(13, 35),
                200,
                175.00m,
                "EUR",
                "Economy",
                0,
                true),

            // Istanbul → Dubai
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000012"),
                "Turkish Airlines",
                "TK760",
                "IST",
                "DXB",
                new DateOnly(2030, 9, 18),
                new TimeOnly(18, 40),
                new TimeOnly(00, 35),
                295,
                280.00m,
                "USD",
                "Economy",
                0,
                true),

            new(
                Guid.Parse("10000000-0000-0000-0000-000000000013"),
                "Emirates",
                "EK124",
                "IST",
                "DXB",
                new DateOnly(2030, 9, 18),
                new TimeOnly(16, 25),
                new TimeOnly(22, 05),
                340,
                920.00m,
                "USD",
                "Business",
                0,
                true),

            // Istanbul → Berlin
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000014"),
                "Turkish Airlines",
                "TK1721",
                "IST",
                "BER",
                new DateOnly(2030, 9, 25),
                new TimeOnly(08, 30),
                new TimeOnly(10, 15),
                165,
                135.00m,
                "EUR",
                "Economy",
                0,
                true),

            new(
                Guid.Parse("10000000-0000-0000-0000-000000000015"),
                "Pegasus",
                "PC979",
                "IST",
                "BER",
                new DateOnly(2030, 9, 25),
                new TimeOnly(11, 20),
                new TimeOnly(16, 05),
                225,
                110.00m,
                "EUR",
                "Economy",
                1,
                true)
        ];

        public IReadOnlyList<Flight> GetAll() => _flights;

        public Flight? GetById(Guid id) =>
            _flights.FirstOrDefault(x => x.Id == id);

        public IReadOnlyList<Flight> Search(
            string? origin,
            string? destination,
            DateOnly? departureDate,
            string? cabinClass,
            decimal? maxPrice,
            int? maxStops)
        {
            return _flights
                .Where(x => origin is null || x.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase))
                .Where(x => destination is null || x.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase))
                .Where(x => departureDate is null || x.DepartureDate == departureDate)
                .Where(x => cabinClass is null || x.CabinClass.Equals(cabinClass, StringComparison.OrdinalIgnoreCase))
                .Where(x => maxPrice is null || x.Price <= maxPrice)
                .Where(x => maxStops is null || x.Stops <= maxStops)
                .Where(x => x.Available)
                .OrderBy(x => x.Price)
                .ToList();
        }
    }
}
