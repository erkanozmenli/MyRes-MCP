using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace MyRes.N8nBootstrap.Workflows;

internal sealed class WorkflowPayloadBuilder(ILogger<WorkflowPayloadBuilder> logger)
{
    private static readonly string[] ServerManagedProperties =
    [
        "id",
        "active",
        "versionId",
        "meta",
        "tags",
    ];

    public async Task<WorkflowPayload> BuildAsync(string sourcePath, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reading and normalizing workflow template from {WorkflowPath}.", sourcePath);

        await using var source = File.OpenRead(sourcePath);
        var root = await JsonNode.ParseAsync(source, cancellationToken: cancellationToken) as JsonObject
            ?? throw new InvalidOperationException("The workflow template must contain a JSON object at its root.");

        if (root["name"] is not JsonValue nameValue ||
            !nameValue.TryGetValue<string>(out var workflowName) ||
            string.IsNullOrWhiteSpace(workflowName))
        {
            throw new InvalidOperationException(
                $"Workflow template '{sourcePath}' must have a non-empty string name.");
        }

        var shouldPublish = false;
        if (root.TryGetPropertyValue("active", out var activeNode))
        {
            if (activeNode is not JsonValue activeValue ||
                !activeValue.TryGetValue<bool>(out shouldPublish))
            {
                throw new InvalidOperationException(
                    $"Workflow template '{sourcePath}' must have a boolean active value when present.");
            }
        }

        foreach (var property in ServerManagedProperties)
        {
            root.Remove(property);
        }

        return new WorkflowPayload(workflowName, shouldPublish, root);
    }
}

internal sealed record WorkflowPayload(string Name, bool ShouldPublish, JsonObject Payload);
