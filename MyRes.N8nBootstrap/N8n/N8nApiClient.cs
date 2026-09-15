using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MyRes.N8nBootstrap.Configuration;

namespace MyRes.N8nBootstrap.N8n;

internal sealed class N8nApiClient(BootstrapOptions options, HttpClient httpClient)
{
    public async Task<IReadOnlyList<N8nWorkflowSummary>> ListWorkflowsAsync(
        CancellationToken cancellationToken)
    {
        var workflows = new List<N8nWorkflowSummary>();
        string? cursor = null;

        do
        {
            var path = "workflows?limit=250";
            if (!string.IsNullOrEmpty(cursor))
            {
                path += $"&cursor={Uri.EscapeDataString(cursor)}";
            }

            using var response = await SendAsync(HttpMethod.Get, path, null, "workflow list", cancellationToken);
            using var document = await ReadJsonAsync(response, "workflow list", cancellationToken);
            var root = document.RootElement;

            if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    "n8n workflow list returned an unexpected response: expected a data array.");
            }

            foreach (var workflow in data.EnumerateArray())
            {
                workflows.Add(ReadWorkflowSummary(workflow, "workflow list"));
            }

            cursor = root.TryGetProperty("nextCursor", out var nextCursor) &&
                     nextCursor.ValueKind == JsonValueKind.String
                ? nextCursor.GetString()
                : null;
        }
        while (!string.IsNullOrEmpty(cursor));

        return workflows;
    }

    public async Task<N8nWorkflowSummary> CreateWorkflowAsync(
        JsonObject payload,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            "workflows",
            payload,
            "workflow create",
            cancellationToken);
        using var document = await ReadJsonAsync(response, "workflow create", cancellationToken);
        return ReadWorkflowSummary(document.RootElement, "workflow create");
    }

    public async Task<N8nWorkflowSummary> UpdateWorkflowAsync(
        string workflowId,
        JsonObject payload,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            $"workflows/{Uri.EscapeDataString(workflowId)}",
            payload,
            "workflow update",
            cancellationToken);
        using var document = await ReadJsonAsync(response, "workflow update", cancellationToken);
        return ReadWorkflowSummary(document.RootElement, "workflow update");
    }

    public async Task PublishWorkflowAsync(string workflowId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Post,
            $"workflows/{Uri.EscapeDataString(workflowId)}/publish",
            null,
            "workflow publish",
            cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePath,
        JsonObject? payload,
        string operation,
        CancellationToken cancellationToken)
    {
        var apiRoot = options.BaseUrl.ToString().TrimEnd('/');
        using var request = new HttpRequestMessage(method, $"{apiRoot}/api/v1/{relativePath}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-N8N-API-KEY", options.ApiKey);

        if (payload is not null)
        {
            request.Content = new StringContent(
                payload.ToJsonString(),
                Encoding.UTF8,
                "application/json");
        }

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = $"n8n {operation} failed with HTTP {(int)response.StatusCode} " +
                      $"({response.StatusCode})";
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            message += $": {responseBody.Trim()}";
        }

        response.Dispose();
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private static async Task<JsonDocument> ReadJsonAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                $"n8n {operation} returned an invalid JSON response.",
                exception);
        }
    }

    private static N8nWorkflowSummary ReadWorkflowSummary(JsonElement workflow, string operation)
    {
        if (!workflow.TryGetProperty("id", out var id) ||
            !workflow.TryGetProperty("name", out var name) ||
            name.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                $"n8n {operation} returned an unexpected workflow response: expected id and name.");
        }

        return new N8nWorkflowSummary(id.ToString(), name.GetString()!);
    }
}

internal sealed record N8nWorkflowSummary(string Id, string Name);
