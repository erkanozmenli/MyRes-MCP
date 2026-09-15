using Microsoft.Extensions.Configuration;
using MyRes.AppHost;

var builder = DistributedApplication.CreateBuilder(args);
var options = new AppHostOptions();
builder.Configuration.Bind(options);

// Redis
const int redisTlsPort = 6379;
const int redisTcpPort = 6380;
var redisPassword = builder.AddParameter("RedisPassword");

var redis = builder
    .AddRedis("redis", port: redisTlsPort, password: redisPassword)
    .WithImage("redis/redis-stack-server")
    .WithImageTag("7.4.0-v1")
    .WithArgs("--loadmodule", "/opt/redis-stack/lib/redisearch.so")
    .WithVolume("myresmcp-redis-data", "/data");

builder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
{
    redis.WithEndpoint(
        endpointName: "secondary",
        callback: endpoint =>
        {
            endpoint.Port = redisTcpPort;
        },
        createIfNotExists: false);

    return Task.CompletedTask;
});


// Redis Insight
builder.AddContainer("redis-insight", "redis/redisinsight")
    .WithReference(redis)
    .WithHttpEndpoint(targetPort: 5540)
    .WithImageTag("3.8.0")
    .WithVolume("myresmcp-redisinsight-data", "/data");


// Provider Service
var providerService = builder.AddProject<Projects.MyRes_ProviderService_Api>("myres-providerservice-api");
var openAiApiKey = builder.AddParameter("OpenAIApiKey", secret: true);
var qdrant = builder.AddQdrant("qdrant")
    .WithDataVolume("myresmcp-qdrant-data");


// Travel Knowledge
var travelKnowledge = builder
    .AddProject<Projects.MyRes_TravelKnowledge_Api>("myres-travelknowledge-api")
    .WithReference(qdrant)
    .WithEnvironment("OPENAI_API_KEY", openAiApiKey)
    .WaitFor(qdrant);


// MCP Server
var mcp = builder.AddProject<Projects.MyRes_Mcp_Api>("myres-mcp-api")
    .WithReference(providerService)
    .WithReference(travelKnowledge)
    .WithReference(redis)
    .WaitFor(redis);

// n8n
var n8n = builder.AddContainer("n8n", "n8nio/n8n", "2.38.6")
    .WithHttpEndpoint(port: 5678, targetPort: 5678)
    .WithVolume("n8n-data", "/home/node/.n8n")
    .WithReference(mcp);


// n8n Bootstrap
var n8nApiKey = builder.AddParameter("N8nApiKey", secret: true);
var n8nWorkflowDirectory = Path.Combine(
    builder.AppHostDirectory,
    "n8n",
    "workflows");

builder.AddProject<Projects.MyRes_N8nBootstrap>("n8n-bootstrap")
    .WithEnvironment("N8N_BASE_URL", n8n.GetEndpoint("http"))
    .WithEnvironment("N8N_API_KEY", n8nApiKey)
    .WithEnvironment("N8N_WORKFLOW_DIRECTORY", n8nWorkflowDirectory)
    .WaitFor(n8n);


// Swtich between Aspire and VSCode for frontend app
if (!options.UseExternalFrontend)
{
    if (string.IsNullOrWhiteSpace(options.N8nChatWebhookId))
    {
        throw new InvalidOperationException("N8nChatWebhookId must be configured when Aspire manages the frontend.");
    }

    builder.AddViteApp("frontend", "../frontend/myres-mcp-ai-test")
        .WithEnvironment("VITE_N8N_CHAT_URL", ReferenceExpression.Create($"{n8n.GetEndpoint("http")}/webhook/{options.N8nChatWebhookId}/chat"))
        .WithEndpoint("http", endpoint => endpoint.Port = 7000);
}

builder.Build().Run();
