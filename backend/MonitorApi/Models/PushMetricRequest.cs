namespace MonitorApi.Models;

public class PushMetricRequest
{
    public string InstanceId { get; set; } = string.Empty;
    public double MemoryPercent { get; set; }
    public double DiskPercent { get; set; }
}
