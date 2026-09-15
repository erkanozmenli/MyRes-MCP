using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyRes.N8nBootstrap.Configuration;
using MyRes.N8nBootstrap.N8n;
using MyRes.N8nBootstrap.Workflows;

var builder = Host.CreateApplicationBuilder(args);

BootstrapOptions options;
try
{
    options = BootstrapOptions.FromConfiguration(builder.Configuration);
}
catch (Exception exception)
{
    Console.Error.WriteLine($"n8n bootstrap configuration error: {exception.Message}");
    return 1;
}

builder.Services.AddSingleton(options);
builder.Services.AddSingleton<N8nApiClient>();
builder.Services.AddSingleton<WorkflowPayloadBuilder>();
builder.Services.AddSingleton<N8nProvisioner>();
builder.Services.AddSingleton(new HttpClient());

using var host = builder.Build();

try
{
    var provisioner = host.Services.GetRequiredService<N8nProvisioner>();
    var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
    await provisioner.ProvisionAsync(lifetime.ApplicationStopping);
    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("n8n bootstrap was cancelled.");
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"n8n bootstrap failed: {exception.Message}");
    return 1;
}
