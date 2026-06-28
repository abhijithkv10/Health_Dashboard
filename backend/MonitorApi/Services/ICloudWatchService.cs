namespace MonitorApi.Services;

public interface ICloudWatchService
{
    Task PollAsync();
}
