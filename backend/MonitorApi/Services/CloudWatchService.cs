using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using MonitorApi.Models;

namespace MonitorApi.Services;

public class CloudWatchService : ICloudWatchService
{
    private readonly IMetricStore _store;
    private readonly ILogger<CloudWatchService> _logger;
    private readonly List<InstanceConfig> _instances;
    private readonly AmazonCloudWatchClient _client;

    public CloudWatchService(IMetricStore store, IConfiguration config, ILogger<CloudWatchService> logger)
    {
        _store = store;
        _logger = logger;
        _instances = config.GetSection("Instances").Get<List<InstanceConfig>>() ?? new();
        var region = config["Aws:Region"] ?? "us-east-1";
        _client = new AmazonCloudWatchClient(Amazon.RegionEndpoint.GetBySystemName(region));
    }

    public async Task PollAsync()
    {
        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddMinutes(-2);

        foreach (var instance in _instances)
        {
            try
            {
                var response = await _client.GetMetricStatisticsAsync(new GetMetricStatisticsRequest
                {
                    Namespace = "AWS/EC2",
                    MetricName = "CPUUtilization",
                    Dimensions = new List<Dimension>
                    {
                        new() { Name = "InstanceId", Value = instance.Id }
                    },
                    StartTimeUtc = startTime,
                    EndTimeUtc = endTime,
                    Period = 60,
                    Statistics = new List<string> { "Average" }
                });

                var datapoint = response.Datapoints.OrderByDescending(d => d.Timestamp).FirstOrDefault();
                if (datapoint != null)
                {
                    var latest = _store.GetLatest(instance.Id);
                    _store.AddMetric(new MetricSnapshot
                    {
                        InstanceId = instance.Id,
                        Timestamp = DateTime.UtcNow,
                        CpuPercent = Math.Round(datapoint.Average, 1),
                        MemoryPercent = latest?.MemoryPercent,
                        DiskPercent = latest?.DiskPercent
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch CPU metrics for {InstanceId}", instance.Id);
            }
        }
    }
}
