using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Reflection;

namespace TradeBotEngine.Functions.Endpoints;

public class Health
{
    /// <summary>GET /api/health - Health check endpoint</summary>
    [Function("HealthCheck")]
    public async Task<HttpResponseData> HealthCheck([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        var health = new
        {
            Status = "Healthy",
            Service = "TradeBotEngine",
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
            Timestamp = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development"
        };

        return await HttpUtils.JsonOk(req, health);
    }

    /// <summary>GET /api/health/config - Check configuration status (no secrets)</summary>
    [Function("ConfigCheck")]
    public async Task<HttpResponseData> ConfigCheck([HttpTrigger(AuthorizationLevel.Function, "get", Route = "health/config")] HttpRequestData req)
    {
        var config = new
        {
            ETrade = new
            {
                ConsumerKeySet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ETrade:ConsumerKey")),
                UseSandbox = Environment.GetEnvironmentVariable("ETrade:UseSandbox") ?? "true"
            },
            AI = new
            {
                Provider = Environment.GetEnvironmentVariable("AI:Provider") ?? "not set",
                ApiKeySet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AI:ApiKey")),
                Model = Environment.GetEnvironmentVariable("AI:Model") ?? "not set"
            },
            Cosmos = new
            {
                ConnectionStringSet = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("Cosmos:ConnectionString")),
                DatabaseName = Environment.GetEnvironmentVariable("Cosmos:DatabaseName") ?? "not set"
            },
            Trading = new
            {
                RiskProfile = Environment.GetEnvironmentVariable("Trading:DefaultRiskProfile") ?? "not set",
                InitialCapital = Environment.GetEnvironmentVariable("Trading:InitialCapital") ?? "not set",
                AutoTradeEnabled = Environment.GetEnvironmentVariable("Trading:AutoTradeEnabled") ?? "false"
            }
        };

        return await HttpUtils.JsonOk(req, config);
    }
}
