namespace MonitorApi.Models;

public class MetricSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InstanceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double? CpuPercent { get; set; }
    public double? MemoryPercent { get; set; }
    public double? DiskPercent { get; set; }
}
