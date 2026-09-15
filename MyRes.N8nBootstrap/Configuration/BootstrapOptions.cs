using Microsoft.Extensions.Configuration;

namespace MyRes.N8nBootstrap.Configuration;

internal sealed record BootstrapOptions(
    Uri BaseUrl,
    string ApiKey,
    string WorkflowDirectory)
{
    public static BootstrapOptions FromConfiguration(IConfiguration configuration)
    {
        var baseUrlValue = Require(configuration, "N8N_BASE_URL");
        var apiKey = Require(configuration, "N8N_API_KEY");
        var workflowDirectory = Path.GetFullPath(Require(configuration, "N8N_WORKFLOW_DIRECTORY"));

        if (!Uri.TryCreate(baseUrlValue, UriKind.Absolute, out var baseUrl) ||
            (baseUrl.Scheme != Uri.UriSchemeHttp && baseUrl.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("N8N_BASE_URL must be an absolute HTTP or HTTPS URL.");
        }

        if (!Directory.Exists(workflowDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The configured n8n workflow directory does not exist or is not a directory: {workflowDirectory}");
        }

        return new BootstrapOptions(
            baseUrl,
            apiKey,
            workflowDirectory);
    }

    private static string Require(IConfiguration configuration, string key) =>
        configuration[key]?.Trim() is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Required configuration value {key} is missing.");

}
