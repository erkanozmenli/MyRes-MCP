using Microsoft.Extensions.Logging;
using MyRes.N8nBootstrap.Configuration;
using MyRes.N8nBootstrap.Workflows;

namespace MyRes.N8nBootstrap.N8n;

internal sealed class N8nProvisioner(
    BootstrapOptions options,
    N8nApiClient apiClient,
    WorkflowPayloadBuilder payloadBuilder,
    HttpClient httpClient,
    ILogger<N8nProvisioner> logger)
{
    private static readonly TimeSpan ReachabilityTimeout = TimeSpan.FromSeconds(120);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public async Task ProvisionAsync(CancellationToken cancellationToken)
    {
        await WaitUntilReadyAsync(cancellationToken);
        var files = Directory.GetFiles(options.WorkflowDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (files.Length == 0)
        {
            throw new InvalidOperationException(
                $"No JSON workflow files were found in '{options.WorkflowDirectory}'.");
        }

        var payloads = new List<WorkflowPayload>(files.Length);
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            var payload = await payloadBuilder.BuildAsync(file, cancellationToken);
            if (!names.Add(payload.Name))
            {
                throw new InvalidOperationException(
                    $"Duplicate workflow name '{payload.Name}' found in local JSON workflow files.");
            }

            payloads.Add(payload);
        }

        var existingWorkflows = (await apiClient.ListWorkflowsAsync(cancellationToken))
            .ToLookup(workflow => workflow.Name, StringComparer.Ordinal);

        foreach (var payload in payloads)
        {
            logger.LogInformation("Provisioning workflow '{WorkflowName}'.", payload.Name);
            var matches = existingWorkflows[payload.Name].ToArray();
            if (matches.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Found {matches.Length} workflows named '{payload.Name}'. " +
                    "Remove duplicates before running bootstrap again.");
            }

            string workflowId;
            if (matches.Length == 0)
            {
                logger.LogInformation("Workflow does not exist; creating it.");
                workflowId = (await apiClient.CreateWorkflowAsync(payload.Payload, cancellationToken)).Id;
            }
            else
            {
                workflowId = matches[0].Id;
                logger.LogInformation("Workflow {WorkflowId} exists; updating it.", workflowId);
                await apiClient.UpdateWorkflowAsync(workflowId, payload.Payload, cancellationToken);
            }

            if (payload.ShouldPublish)
            {
                logger.LogInformation("Publishing workflow {WorkflowId}.", workflowId);
                await apiClient.PublishWorkflowAsync(workflowId, cancellationToken);
            }
            else
            {
                logger.LogInformation("Workflow '{WorkflowName}' is configured as unpublished; skipping publish.", payload.Name);
            }
            logger.LogInformation("Workflow '{WorkflowName}' completed.", payload.Name);
        }

        logger.LogInformation("n8n workflow provisioning completed successfully.");
    }

    private async Task WaitUntilReadyAsync(CancellationToken cancellationToken)
    {
        var readinessUrl = $"{options.BaseUrl.ToString().TrimEnd('/')}/healthz/readiness";
        var deadline = DateTimeOffset.UtcNow + ReachabilityTimeout;
        logger.LogInformation("Waiting for n8n to become ready at {N8nBaseUrl}.", options.BaseUrl);

        while (true)
        {
            try
            {
                using var response = await httpClient.GetAsync(readinessUrl, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("n8n is ready.");
                    return;
                }

                logger.LogDebug("n8n readiness check returned HTTP {StatusCode}.", (int)response.StatusCode);
            }
            catch (HttpRequestException exception)
            {
                logger.LogDebug(exception, "n8n is not ready yet.");
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                throw new TimeoutException(
                    $"n8n did not become ready within {ReachabilityTimeout.TotalSeconds:0} seconds.");
            }

            await Task.Delay(RetryDelay, cancellationToken);
        }
    }
}
