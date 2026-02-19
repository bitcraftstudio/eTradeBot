using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TradeBotEngine.Core.Interfaces;

namespace TradeBotEngine.Functions.Endpoints;

public class Scheduler
{
    private readonly ILogger<Scheduler> _logger;
    private readonly IPositionMonitorService _positionMonitor;
    private readonly ILearningService _learningService;
    private readonly IMarketScanService _marketScanService;
    private readonly ISchedulerConfigRepository _configRepository;

    public Scheduler(
        IPositionMonitorService positionMonitor,
        ILearningService learningService,
        IMarketScanService marketScanService,
        ISchedulerConfigRepository configRepository,
        ILogger<Scheduler> logger)
    {
        _positionMonitor = positionMonitor;
        _learningService = learningService;
        _marketScanService = marketScanService;
        _configRepository = configRepository;
        _logger = logger;
    }

    /// <summary>GET /api/scheduler/status - Get scheduler status</summary>
    [Function("GetSchedulerStatus")]
    public async Task<HttpResponseData> GetSchedulerStatus([HttpTrigger(AuthorizationLevel.Function, "get", Route = "scheduler/status")] HttpRequestData req)
    {
        _logger.LogInformation("GET /scheduler/status called");
        var config = await _configRepository.GetConfigAsync();
        
        var status = new
        {
            IsRunning = config.SchedulerEnabled,
            config.SchedulerEnabled,
            config.MorningScanEnabled,
            config.PositionMonitoringEnabled,
            config.DailyLearningEnabled,
            config.LastScanTime,
            config.LastMonitorTime,
            config.LastLearningTime,
            Config = config
        };
        
        _logger.LogInformation("Returning status with {Count} watchlist items", config.Watchlist?.Count ?? 0);
        return await HttpUtils.JsonOk(req, status);
    }

    /// <summary>GET /api/scheduler/config - Get scheduler config</summary>
    [Function("GetSchedulerConfig")]
    public async Task<HttpResponseData> GetSchedulerConfig([HttpTrigger(AuthorizationLevel.Function, "get", Route = "scheduler/config")] HttpRequestData req)
    {
        _logger.LogInformation("GET /scheduler/config called");
        var config = await _configRepository.GetConfigAsync();
        _logger.LogInformation("Returning config with {Count} watchlist items, minConfidence={MinConf}", 
            config.Watchlist?.Count ?? 0, config.MinConfidence);
        return await HttpUtils.JsonOk(req, config);
    }

    /// <summary>PATCH /api/scheduler/config - Update scheduler config</summary>
    [Function("UpdateSchedulerConfig")]
    public async Task<HttpResponseData> UpdateSchedulerConfig([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "scheduler/config")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            _logger.LogInformation("PATCH /scheduler/config called with body: {Body}", body);
            
            if (string.IsNullOrEmpty(body))
            {
                return await HttpUtils.JsonOther(req, HttpStatusCode.BadRequest, "Request body required");
            }

            var updates = JsonSerializer.Deserialize<SchedulerConfigUpdate>(body, HttpUtils.JsonInOpts);
            
            _logger.LogInformation("Parsed updates - Watchlist count: {Count}, MinConfidence: {MinConf}", 
                updates?.Watchlist?.Count ?? -1, updates?.MinConfidence);
            
            // Get current config from database
            var config = await _configRepository.GetConfigAsync();
            _logger.LogInformation("Current config - Watchlist: {Count} items, MinConfidence: {MinConf}", 
                config.Watchlist?.Count ?? 0, config.MinConfidence);
            
            // Apply ALL updates - check for presence in JSON, not just null
            if (updates != null)
            {
                // Watchlist - explicitly allow empty array
                if (updates.Watchlist != null)
                {
                    _logger.LogInformation("Updating watchlist from {Old} items to {New} items", 
                        config.Watchlist?.Count ?? 0, updates.Watchlist.Count);
                    config.Watchlist = updates.Watchlist;
                }
                
                // Boolean toggles
                if (updates.SchedulerEnabled.HasValue)
                    config.SchedulerEnabled = updates.SchedulerEnabled.Value;
                if (updates.MorningScanEnabled.HasValue) 
                    config.MorningScanEnabled = updates.MorningScanEnabled.Value;
                if (updates.PositionMonitoringEnabled.HasValue) 
                    config.PositionMonitoringEnabled = updates.PositionMonitoringEnabled.Value;
                if (updates.DailyLearningEnabled.HasValue) 
                    config.DailyLearningEnabled = updates.DailyLearningEnabled.Value;
                if (updates.AutoTradeEnabled.HasValue) 
                    config.AutoTradeEnabled = updates.AutoTradeEnabled.Value;
                
                // String/numeric settings
                if (!string.IsNullOrEmpty(updates.MorningScanTime)) 
                    config.MorningScanTime = updates.MorningScanTime;
                if (updates.PositionCheckInterval.HasValue) 
                    config.PositionCheckInterval = updates.PositionCheckInterval.Value;
                if (updates.MaxDailyTrades.HasValue) 
                    config.MaxDailyTrades = updates.MaxDailyTrades.Value;
                if (updates.MinConfidence.HasValue)
                {
                    _logger.LogInformation("Updating MinConfidence from {Old} to {New}", 
                        config.MinConfidence, updates.MinConfidence.Value);
                    config.MinConfidence = updates.MinConfidence.Value;
                }
            }

            // Save and get the result
            var savedConfig = await _configRepository.SaveConfigAsync(config);
            _logger.LogInformation("Saved config - Watchlist: {Count} items, MinConfidence: {MinConf}", 
                savedConfig.Watchlist?.Count ?? 0, savedConfig.MinConfidence);

            return await HttpUtils.JsonOk(req, new { message = "Config updated", config = savedConfig });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update scheduler config");
            return await HttpUtils.JsonOther(req, HttpStatusCode.BadRequest, ex.Message);
        }
    }

    /// <summary>POST /api/scheduler/scan - Trigger manual market scan</summary>
    [Function("ManualMarketScan")]
    public async Task<HttpResponseData> ManualMarketScan([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/scan")] HttpRequestData req)
    {
        _logger.LogInformation("Manual market scan triggered via HTTP");
        var result = await _marketScanService.PerformMarketScanAsync();
        
        // Update last scan time
        var config = await _configRepository.GetConfigAsync();
        config.LastScanTime = DateTime.UtcNow;
        await _configRepository.SaveConfigAsync(config);
        
        return await HttpUtils.JsonOk(req, result);
    }

    /// <summary>POST /api/scheduler/monitor - Trigger manual position check</summary>
    [Function("ManualPositionMonitor")]
    public async Task<HttpResponseData> ManualPositionMonitor([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/monitor")] HttpRequestData req)
    {
        await _positionMonitor.UpdateAllPositionsAsync();
        var signals = await _positionMonitor.CheckSellSignalsAsync();
        
        // Update last monitor time
        var config = await _configRepository.GetConfigAsync();
        config.LastMonitorTime = DateTime.UtcNow;
        await _configRepository.SaveConfigAsync(config);
        
        return await HttpUtils.JsonOk(req, new { signals });
    }

    /// <summary>POST /api/scheduler/learn - Trigger manual learning review</summary>
    [Function("ManualLearningReview")]
    public async Task<HttpResponseData> ManualLearningReview([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/learn")] HttpRequestData req)
    {
        var review = await _learningService.PerformDailyReviewAsync(30);
        
        // Update last learning time
        var config = await _configRepository.GetConfigAsync();
        config.LastLearningTime = DateTime.UtcNow;
        await _configRepository.SaveConfigAsync(config);
        
        return await HttpUtils.JsonOk(req, review);
    }

    // Enable/Disable endpoints
    
    [Function("EnableScheduler")]
    public async Task<HttpResponseData> EnableScheduler([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/enable")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.SchedulerEnabled = true;
        await _configRepository.SaveConfigAsync(config);
        _logger.LogInformation("Scheduler enabled");
        return await HttpUtils.JsonOk(req, new { message = "Scheduler enabled", enabled = true });
    }

    [Function("DisableScheduler")]
    public async Task<HttpResponseData> DisableScheduler([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/disable")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.SchedulerEnabled = false;
        await _configRepository.SaveConfigAsync(config);
        _logger.LogInformation("Scheduler disabled");
        return await HttpUtils.JsonOk(req, new { message = "Scheduler disabled", enabled = false });
    }

    [Function("EnableMorningScan")]
    public async Task<HttpResponseData> EnableMorningScan([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/enable/morning-scan")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.MorningScanEnabled = true;
        await _configRepository.SaveConfigAsync(config);
        return await HttpUtils.JsonOk(req, new { message = "Morning scan enabled", enabled = true });
    }

    [Function("DisableMorningScan")]
    public async Task<HttpResponseData> DisableMorningScan([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/disable/morning-scan")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.MorningScanEnabled = false;
        await _configRepository.SaveConfigAsync(config);
        return await HttpUtils.JsonOk(req, new { message = "Morning scan disabled", enabled = false });
    }

    [Function("EnablePositionMonitoring")]
    public async Task<HttpResponseData> EnablePositionMonitoring([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/enable/position-monitoring")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.PositionMonitoringEnabled = true;
        await _configRepository.SaveConfigAsync(config);
        return await HttpUtils.JsonOk(req, new { message = "Position monitoring enabled", enabled = true });
    }

    [Function("DisablePositionMonitoring")]
    public async Task<HttpResponseData> DisablePositionMonitoring([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/disable/position-monitoring")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.PositionMonitoringEnabled = false;
        await _configRepository.SaveConfigAsync(config);
        return await HttpUtils.JsonOk(req, new { message = "Position monitoring disabled", enabled = false });
    }

    [Function("EnableDailyLearning")]
    public async Task<HttpResponseData> EnableDailyLearning([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/enable/daily-learning")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.DailyLearningEnabled = true;
        await _configRepository.SaveConfigAsync(config);
        return await HttpUtils.JsonOk(req, new { message = "Daily learning enabled", enabled = true });
    }

    [Function("DisableDailyLearning")]
    public async Task<HttpResponseData> DisableDailyLearning([HttpTrigger(AuthorizationLevel.Function, "post", Route = "scheduler/disable/daily-learning")] HttpRequestData req)
    {
        var config = await _configRepository.GetConfigAsync();
        config.DailyLearningEnabled = false;
        await _configRepository.SaveConfigAsync(config);
        return await HttpUtils.JsonOk(req, new { message = "Daily learning disabled", enabled = false });
    }

    /// <summary>DELETE /api/scheduler/config - Reset config to defaults (empty watchlist)</summary>
    [Function("ResetSchedulerConfig")]
    public async Task<HttpResponseData> ResetSchedulerConfig([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "scheduler/config")] HttpRequestData req)
    {
        _logger.LogInformation("DELETE /scheduler/config - Resetting config to defaults");
        var config = await _configRepository.ResetConfigAsync();
        _logger.LogInformation("Config reset. Watchlist: {Count} items, MinConfidence: {MinConf}", 
            config.Watchlist?.Count ?? 0, config.MinConfidence);
        return await HttpUtils.JsonOk(req, new { message = "Config reset to defaults", config });
    }
}
