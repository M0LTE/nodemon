using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using nodemon.Configuration;
using System.Diagnostics;
using tait_ccdi;

namespace nodemon.Services;

public class TaitSingleton
{
    public Dictionary<string, TaitRadio> Radios = [];
}

public class TaitManager(IOptions<NodeMonConfig> config, ILogger<TaitManager> logger, IHubContext<NodeHub> hubContext, TaitSingleton taitSingleton) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (config.Value == null || config.Value.Ports == null || config.Value.Ports.Length == 0)
        {
            logger.LogWarning("No ports configured");
        }

        foreach (var port in config.Value!.Ports!)
        {
            if (port.Skip) continue;

            await Run(port);
        }
    }

    private Task Run(NodeMonConfig.Port port)
    {
        var lastRssiReported = Stopwatch.StartNew();
        try
        {
            logger.LogInformation("Opening port {port} {radioPort}", port.Id, port.RadioPort);

            TaitRadio radio = new(port.RadioPort, port.RadioBaud, logger);
            taitSingleton.Radios.Add(port.Id, radio);

            radio.RawRssiUpdated += async (sender, args) =>
            {
                await hubContext.Clients.All.SendAsync("RssiUpdate", new { r = args.Rssi, p = port.Id });

                if (lastRssiReported.Elapsed > TimeSpan.FromSeconds(10))
                {
                    logger.LogInformation("{port} RSSI: {rssi}", port.Id, args.Rssi);
                    lastRssiReported.Restart();
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

            radio.PaTempRead += (sender, args) =>
            {
                logger.LogInformation("{port} PA Temp: {temp}", port.Id, args.TempC);
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
