using Microsoft.Extensions.Options;
using nodemon.Configuration;
using tait_ccdi;

namespace nodemon.Services;

public class TaitManager(IOptions<NodeMonConfig> config, ILogger<TaitManager> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (config.Value == null || config.Value.Ports == null || config.Value.Ports.Length == 0)
        {
            logger.LogWarning("No ports configured");
            return Task.CompletedTask;
        }

        foreach (var port in config.Value.Ports)
        {
            _ = Task.Run(() => RunPort(port), cancellationToken);
        }

        return Task.CompletedTask;
    }

    private async void RunPort(NodeMonConfig.Port port)
    {
        while (true)
        {
            try
            {
                logger.LogInformation("Opening port {port} {radioPort}", port.Id, port.RadioPort);
                
                TaitRadio radio = new(port.RadioPort, port.RadioBaud);
                
                while(true)
                {
                    var rssi = radio.GetRawRssi();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
            }
            finally
            {
                await Task.Delay(10000);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
