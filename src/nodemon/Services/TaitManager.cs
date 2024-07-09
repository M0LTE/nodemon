using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using nodemon.Configuration;
using System.Diagnostics;
using tait_ccdi;

namespace nodemon.Services;

public class TaitManager(IOptions<NodeMonConfig> config, ILogger<TaitManager> logger, IHubContext<NodeHub> hubContext) : IHostedService
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

    private List<TaitRadio> radios = new();

    private Task Run(NodeMonConfig.Port port)
    {
        var lastReported = Stopwatch.StartNew();
        try
        {
            logger.LogInformation("Opening port {port} {radioPort}", port.Id, port.RadioPort);

            TaitRadio radio = TaitRadio.Create(port.RadioPort, port.RadioBaud, logger);
            radios.Add(radio);

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
