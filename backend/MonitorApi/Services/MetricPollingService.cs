namespace MonitorApi.Services;

public class MetricPollingService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MetricPollingService> _logger;
    private readonly int _intervalSeconds;

    public MetricPollingService(IServiceProvider services, IConfiguration config, ILogger<MetricPollingService> logger)
    {
        _services = services;
        _logger = logger;
        _intervalSeconds = int.Parse(config["Monitoring:PollingIntervalSeconds"] ?? "60");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var alerts = scope.ServiceProvider.GetRequiredService<IAlertService>();

                await alerts.EvaluateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in metric polling cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
        }
    }
}
