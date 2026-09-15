namespace MyRes.TravelKnowledge.Infrastructure.Seeding;

public sealed class TravelKnowledgeDocumentParser
{
    internal TravelKnowledgeDocument Parse(string text)
    {
        var normalized = text.Replace("\r\n", "\n");
        var separator = normalized.IndexOf("\n---\n", StringComparison.Ordinal);
        if (separator < 0)
            throw new FormatException("Seed document metadata must end with '---'.");

        var metadata = normalized[..separator]
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split(':', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);

        return new TravelKnowledgeDocument(
            Required("Title"), Required("Country").ToUpperInvariant(),
            Required("Topic").ToLowerInvariant(), Required("Source"),
            metadata.GetValueOrDefault("SourceUrl"), normalized[(separator + 5)..].Trim());

        string Required(string key) => metadata.TryGetValue(key, out var value) && value.Length > 0
            ? value
            : throw new FormatException($"Seed document is missing '{key}'.");
    }
}
