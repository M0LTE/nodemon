using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using nodemon.Configuration;
using System.Diagnostics;
using tait_ccdi;

namespace nodemon.Services;

public class TaitManager(IOptions<NodeMonConfig> config, ILogger<TaitManager> logger, IHubContext<NodeHub> hubContext) : IHostedService
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
            if (port.Skip) continue;

            _ = Task.Run(() => RunPort(port), cancellationToken);
        }

        return Task.CompletedTask;
    }

    private async void RunPort(NodeMonConfig.Port port)
    {
        while (true)
        {
            var lastReported = Stopwatch.StartNew();
            try
            {
                logger.LogInformation("Opening port {port} {radioPort}", port.Id, port.RadioPort);
                
                TaitRadio radio = new(port.RadioPort, port.RadioBaud, logger);
                radio.RawRssiUpdated += async (sender, args) =>
                {
                    await hubContext.Clients.All.SendAsync("RssiUpdate", args.Rssi);

                    if (lastReported.Elapsed > TimeSpan.FromSeconds(10))
                    {
                        logger.LogInformation("{port} RSSI: {rssi}", port.Id, args.Rssi);
                        lastReported.Restart();
                    }
                };

                radio.StateChanged += (sender, args) =>
                {
                    logger.LogInformation("{port} State: {state}", port.Id, args.To);
                };

                radio.VswrChanged += (sender, args) =>
                {
                    logger.LogInformation("{port} VSWR: {vswr}", port.Id, args.Vswr);
                };
                
                radio.StartGetRawRssi();

                await radio.Run();
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
