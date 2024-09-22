using System.Diagnostics;

namespace nodemon.Services;

public class RegularTelemetryBeaconService(BeaconService beaconService, TelemetrySingleton telemetrySingleton, ILogger<RegularTelemetryBeaconService> logger) : IHostedService
{
    private Timer? _timer = null;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

        _ = Task.Run(async () =>
        {
            ///TODO: remove hard coded values

            await beaconService.SendAprsTelemetrySeriesLabels("M0LTE-2", "APRS", "2m", "Chassis temp", "Humidity");
            await Task.Delay(5000, stoppingToken);
            await beaconService.SendAprsTelemetrySeriesUnits("M0LTE-2", "APRS", "2m", "C", "%");
        }, stoppingToken);

        return Task.CompletedTask;
    }

    private int seq = 10;

    private void DoWork(object? state)
    {
        if (ShouldSend())
        {
            beaconService.SendAprsTelemetryValues("M0LTE-2", "APRS", "2m", seq,
                telemetrySingleton.ChassisTemperature!.Value,
                telemetrySingleton.ChassisHumidity!.Value);

            lastSent.Restart();
            lastChassisHumidity = telemetrySingleton.ChassisHumidity.Value;
            lastChassisTemp = telemetrySingleton.ChassisTemperature.Value;

            Interlocked.Increment(ref seq);
        }
    }


    private readonly Stopwatch lastSent = new();
    private int lastChassisTemp;
    private int lastChassisHumidity;

    private bool ShouldSend()
    {
        if (telemetrySingleton.ChassisHumidity == null || telemetrySingleton.ChassisTemperature == null)
        {
            logger.LogWarning("No telemetry data available");
            return false;
        }

        if (lastChassisHumidity != telemetrySingleton.ChassisHumidity.Value || lastChassisTemp != telemetrySingleton.ChassisTemperature.Value)
        {
            logger.LogInformation("Telemetry data changed since last send");
            return true;
        }

        if (!lastSent.IsRunning || lastSent.Elapsed > TimeSpan.FromMinutes(15))
        {
            logger.LogInformation("Telemetry data has not been sent for 15 minutes");
            return true;
        }

        logger.LogInformation("Telemetry data has not changed since last send, and timer has not elapsed");
        return false;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
