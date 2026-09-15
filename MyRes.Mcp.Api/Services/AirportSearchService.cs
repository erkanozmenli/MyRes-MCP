using MyRes.Mcp.Api.Models;
using StackExchange.Redis;
using System.Globalization;
using System.Text;

namespace MyRes.Mcp.Api.Services
{
    public sealed class AirportSearchService(IConnectionMultiplexer redis)
    {
        private const string IndexName = "idx:airports";
        private const string KeyPrefix = "airport:";

        private static readonly IReadOnlyList<Airport> SeedAirports =
        [
            new("IST", "Istanbul Airport", "Istanbul"),
            new("SAW", "Sabiha Gokcen International Airport", "Istanbul"),
            new("JFK", "John F. Kennedy International Airport", "New York"),
            new("EWR", "Newark Liberty International Airport", "New York"),
            new("LGA", "LaGuardia Airport", "New York")
        ];

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var database = redis.GetDatabase();

            await EnsureIndexAsync(database, cancellationToken);

            foreach (var airport in SeedAirports)
            {
                HashEntry[] fields =
                [
                    new("code", airport.Code),
                    new("name", airport.Name),
                    new("city", airport.City),
                    new("normalizedName", NormalizeText(airport.Name)),
                    new("normalizedCity", NormalizeText(airport.City)),
                    new("normalizedNameKey", NormalizeKey(airport.Name)),
                    new("normalizedCityKey", NormalizeKey(airport.City))
                ];

                await database
                    .HashSetAsync($"{KeyPrefix}{airport.Code}", fields)
                    .WaitAsync(cancellationToken);
            }
        }

        public async Task<IReadOnlyList<Airport>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return [];

            var database = redis.GetDatabase();
            var normalizedText = NormalizeText(query);
            var normalizedKey = NormalizeKey(query);

            if (IsIataCode(normalizedText))
            {
                var codeMatches = await SearchIndexAsync(
                    database,
                    $"@code:{{{normalizedText.ToUpperInvariant()}}}",
                    cancellationToken);

                if (codeMatches.Count > 0)
                    return codeMatches;
            }

            var nameMatches = await SearchIndexAsync(
                database,
                $"@normalizedNameKey:{{{EscapeTagValue(normalizedKey)}}}",
                cancellationToken);

            if (nameMatches.Count > 0)
                return nameMatches;

            var cityMatches = await SearchIndexAsync(
                database,
                $"@normalizedCityKey:{{{EscapeTagValue(normalizedKey)}}}",
                cancellationToken);

            if (cityMatches.Count > 0)
                return cityMatches;

            var fuzzyQuery = string.Join(
                " ",
                normalizedText
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(BuildFuzzyTerm));

            return await SearchIndexAsync(database, fuzzyQuery, cancellationToken);
        }

        private static async Task EnsureIndexAsync(
            IDatabase database,
            CancellationToken cancellationToken)
        {
            try
            {
                await database
                    .ExecuteAsync("FT.INFO", IndexName)
                    .WaitAsync(cancellationToken);
            }
            catch (RedisServerException exception) when (
                exception.Message.Contains("Unknown Index name", StringComparison.OrdinalIgnoreCase) ||
                exception.Message.Contains("no such index", StringComparison.OrdinalIgnoreCase))
            {
                await database.ExecuteAsync(
                        "FT.CREATE",
                        IndexName,
                        "ON", "HASH",
                        "PREFIX", "1", KeyPrefix,
                        "SCHEMA",
                        "code", "TAG",
                        "name", "TEXT", "NOSTEM",
                        "city", "TEXT", "NOSTEM",
                        "normalizedName", "TEXT", "NOSTEM",
                        "normalizedCity", "TEXT", "NOSTEM",
                        "normalizedNameKey", "TAG",
                        "normalizedCityKey", "TAG")
                    .WaitAsync(cancellationToken);
            }
        }

        private static async Task<IReadOnlyList<Airport>> SearchIndexAsync(
            IDatabase database,
            string query,
            CancellationToken cancellationToken)
        {
            var result = await database.ExecuteAsync(
                    "FT.SEARCH",
                    IndexName,
                    query,
                    "NOCONTENT",
                    "LIMIT", "0", "20")
                .WaitAsync(cancellationToken);

            var values = (RedisResult[]?)result;

            if (values is null || values.Length <= 1)
                return [];

            var airports = new List<Airport>(values.Length - 1);

            foreach (var value in values.Skip(1))
            {
                var fields = await database
                    .HashGetAllAsync((RedisKey)value.ToString())
                    .WaitAsync(cancellationToken);
                var valuesByName = fields.ToDictionary(
                    field => field.Name.ToString(),
                    field => field.Value.ToString(),
                    StringComparer.Ordinal);

                if (valuesByName.TryGetValue("code", out var code) &&
                    valuesByName.TryGetValue("name", out var name) &&
                    valuesByName.TryGetValue("city", out var city))
                    airports.Add(new Airport(code, name, city));
            }

            return airports;
        }

        private static string BuildFuzzyTerm(string term)
        {
            var marker = term.Length >= 4 ? "%%" : "%";
            return $"@normalizedName|normalizedCity|name|city:({marker}{term}{marker})";
        }

        private static bool IsIataCode(string value) =>
            value.Length == 3 && value.All(character => character is >= 'a' and <= 'z');

        private static string NormalizeText(string value)
        {
            var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
            var result = new StringBuilder(decomposed.Length);
            var separatorPending = false;

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(character))
                {
                    if (separatorPending && result.Length > 0)
                        result.Append(' ');

                    result.Append(char.ToLowerInvariant(character));
                    separatorPending = false;
                }
                else
                {
                    separatorPending = true;
                }
            }

            return result.ToString();
        }

        private static string NormalizeKey(string value) =>
            NormalizeText(value).Replace(' ', '-');

        private static string EscapeTagValue(string value) =>
            value.Replace("-", "\\-");
    }
}
