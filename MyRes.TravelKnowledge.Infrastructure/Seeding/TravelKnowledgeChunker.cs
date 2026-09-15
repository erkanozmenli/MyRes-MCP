using System.Security.Cryptography;
using System.Text;
using MyRes.TravelKnowledge.Application;

namespace MyRes.TravelKnowledge.Infrastructure.Seeding;

public sealed class TravelKnowledgeChunker
{
    private const int TargetCharacters = 2800;
    private const int OverlapCharacters = 400;

    internal IReadOnlyList<TravelKnowledgeChunk> Chunk(TravelKnowledgeDocument document)
    {
        var sections = SplitSections(document.Body);
        var chunks = new List<TravelKnowledgeChunk>();

        foreach (var (heading, content) in sections)
        {
            foreach (var part in SplitContent(content))
            {
                var idSource = $"{document.Country}|{document.Topic}|{document.Title}|{heading}|{part}";
                chunks.Add(new TravelKnowledgeChunk(
                    DeterministicGuid(idSource), part, document.Country, document.Topic,
                    document.Source, document.SourceUrl, document.Title, heading));
            }
        }

        return chunks;
    }

    private static IEnumerable<(string? Heading, string Content)> SplitSections(string body)
    {
        string? heading = null;
        var content = new StringBuilder();
        foreach (var line in body.Split('\n'))
        {
            if (line.StartsWith("# ") || line.StartsWith("## ") || line.StartsWith("### "))
            {
                if (content.Length > 0)
                    yield return (heading, content.ToString().Trim());
                heading = line.TrimStart('#', ' ');
                content.Clear();
            }
            else
            {
                content.AppendLine(line);
            }
        }
        if (content.Length > 0)
            yield return (heading, content.ToString().Trim());
    }

    private static IEnumerable<string> SplitContent(string content)
    {
        if (content.Length <= TargetCharacters)
        {
            if (content.Length > 0) yield return content;
            yield break;
        }

        var paragraphs = content.Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var current = new StringBuilder();
        foreach (var paragraph in paragraphs)
        {
            if (current.Length > 0 && current.Length + paragraph.Length + 2 > TargetCharacters)
            {
                var completed = current.ToString().Trim();
                yield return completed;
                current.Clear();
                var overlapStart = Math.Max(0, completed.Length - OverlapCharacters);
                current.Append(completed[overlapStart..]).AppendLine().AppendLine();
            }
            current.Append(paragraph).AppendLine().AppendLine();
        }
        if (current.Length > 0) yield return current.ToString().Trim();
    }

    private static Guid DeterministicGuid(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }
}
