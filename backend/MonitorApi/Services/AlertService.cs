using System.Collections.Concurrent;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using MonitorApi.Models;

namespace MonitorApi.Services;

public class AlertService : IAlertService
{
    private readonly IMetricStore _store;
    private readonly IConfiguration _config;
    private readonly ILogger<AlertService> _logger;
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly string? _snsTopicArn;
    private readonly ConcurrentDictionary<string, string> _previousStates = new();

    private readonly double _cpuCritical;
    private readonly int _cpuDurationMinutes;
    private readonly double _memoryCritical;
    private readonly double _diskCritical;
    private readonly double _cpuWarning;
    private readonly double _memoryWarning;
    private readonly double _diskWarning;

    public AlertService(IMetricStore store, IConfiguration config, ILogger<AlertService> logger)
    {
        _store = store;
        _config = config;
        _logger = logger;

        _cpuCritical = double.Parse(config["Monitoring:CpuCriticalThreshold"] ?? "80");
        _cpuDurationMinutes = int.Parse(config["Monitoring:CpuCriticalDurationMinutes"] ?? "3");
        _memoryCritical = double.Parse(config["Monitoring:MemoryCriticalThreshold"] ?? "80");
        _diskCritical = double.Parse(config["Monitoring:DiskCriticalThreshold"] ?? "90");
        _cpuWarning = double.Parse(config["Monitoring:CpuWarningThreshold"] ?? "60");
        _memoryWarning = double.Parse(config["Monitoring:MemoryWarningThreshold"] ?? "70");
        _diskWarning = double.Parse(config["Monitoring:DiskWarningThreshold"] ?? "80");

        _snsTopicArn = config["Aws:SnsTopicArn"];
        var region = config["Aws:Region"] ?? "us-east-1";

        if (!string.IsNullOrEmpty(_snsTopicArn))
        {
            _snsClient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.GetBySystemName(region));
        }
        else
        {
            _snsClient = null!;
        }
    }

    public async Task EvaluateAsync()
    {
        var instanceIds = await _store.GetAllInstanceIdsAsync();
        var configuredInstances = _config.GetSection("Instances").Get<List<InstanceConfig>>() ?? new();

        foreach (var instanceId in instanceIds)
        {
            var config = configuredInstances.FirstOrDefault(c => c.Id == instanceId);
            var name = config?.Name ?? instanceId;
            var metrics = await _store.GetMetricsAsync(instanceId, _cpuDurationMinutes);
            var latest = metrics.LastOrDefault();
            if (latest == null) continue;

            var cpuCritical = IsCpuCritical(metrics);
            var memoryCritical = latest.MemoryPercent > _memoryCritical;
            var diskCritical = latest.DiskPercent > _diskCritical;

            var cpuWarning = !cpuCritical && latest.CpuPercent > _cpuWarning;
            var memoryWarning = !memoryCritical && latest.MemoryPercent > _memoryWarning;
            var diskWarning = !diskCritical && latest.DiskPercent > _diskWarning;

            string newState;
            var alerts = new List<string>();

            if (cpuCritical) alerts.Add($"CPU at {latest.CpuPercent}% (>{_cpuCritical}% for {_cpuDurationMinutes} min)");
            if (memoryCritical) alerts.Add($"Memory at {latest.MemoryPercent}% (>{_memoryCritical}%)");
            if (diskCritical) alerts.Add($"Disk at {latest.DiskPercent}% (>{_diskCritical}%)");

            if (alerts.Count > 0)
                newState = "Critical";
            else if (cpuWarning || memoryWarning || diskWarning)
                newState = "Warning";
            else
                newState = "OK";

            var previousState = _previousStates.GetOrAdd(instanceId, "OK");

            if (newState == "Critical" && previousState != "Critical")
            {
                var message = $"ALERT: {name} ({instanceId}) is CRITICAL\n" +
                              string.Join("\n", alerts) + "\n" +
                              $"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}";
                await SendNotification($"CRITICAL: {name}", message);
            }
            else if (newState == "OK" && previousState == "Critical")
            {
                var message = $"RESOLVED: {name} ({instanceId}) is now OK\n" +
                              $"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}";
                await SendNotification($"RESOLVED: {name}", message);
            }

            _previousStates[instanceId] = newState;
        }
    }

    private bool IsCpuCritical(List<MetricSnapshot> metrics)
    {
        if (metrics.Count < _cpuDurationMinutes) return false;
        var recent = metrics.TakeLast(_cpuDurationMinutes).ToList();
        return recent.Count >= _cpuDurationMinutes && recent.All(m => m.CpuPercent > _cpuCritical);
    }

    private async Task SendNotification(string subject, string message)
    {
        if (_snsClient == null || string.IsNullOrEmpty(_snsTopicArn))
        {
            _logger.LogInformation("SNS not configured. Would send: {Subject} - {Message}", subject, message);
            return;
        }

        try
        {
            await _snsClient.PublishAsync(new PublishRequest
            {
                TopicArn = _snsTopicArn,
                Subject = subject,
                Message = message
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send SNS notification");
        }
    }
}
