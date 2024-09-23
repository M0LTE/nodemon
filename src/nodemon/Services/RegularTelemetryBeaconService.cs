using System.Diagnostics;

namespace nodemon.Services;

public class RegularTelemetryBeaconService(BeaconService beaconService, TelemetrySingleton telemetrySingleton, ILogger<RegularTelemetryBeaconService> logger) : IHostedService
{
    private const string persistenceKey = "telemetryBeaconSeq";

    private Timer? _timer = null;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));

        _ = Task.Run(async () =>
        {
            ///TODO: remove hard coded values

            await beaconService.SendAprsTelemetrySeriesLabels("M0LTE-2", "APRS", "2m", "Chassis temp", "Humidity", "PA temp 1", "PA temp 2");
            await Task.Delay(5000, stoppingToken);
            await beaconService.SendAprsTelemetrySeriesUnits("M0LTE-2", "APRS", "2m", "C", "%", "C", "C");
        }, stoppingToken);

        seq = PersistentStateService.RestoreInt(persistenceKey, 160);

        return Task.CompletedTask;
    }

    private int seq;

    private void DoWork(object? state)
    {
        if (ShouldSend())
        {
            beaconService.SendAprsTelemetryValues("M0LTE-2", "APRS", "2m", seq,
                telemetrySingleton.ChassisTemperature!.Value,
                telemetrySingleton.ChassisHumidity!.Value,
                telemetrySingleton.PaTemp1!.Value,
                telemetrySingleton.PaTemp2!.Value);

            lastSent.Restart();
            lastChassisHumidity = telemetrySingleton.ChassisHumidity.Value;
            lastChassisTemp = telemetrySingleton.ChassisTemperature.Value;
            lastPaTemp1 = telemetrySingleton.PaTemp1.Value;
            lastPaTemp2 = telemetrySingleton.PaTemp2.Value;

            Interlocked.Increment(ref seq);
            PersistentStateService.SaveInt(persistenceKey, seq);
        }
    }

    private readonly Stopwatch lastSent = new();
    private int lastChassisTemp;
    private int lastChassisHumidity;
    private int lastPaTemp1;
    private int lastPaTemp2;

    private bool ShouldSend()
    {
        //TODO: only check for configured PA temp channels

        if (telemetrySingleton.ChassisHumidity == null || telemetrySingleton.ChassisTemperature == null || telemetrySingleton.PaTemp1 == null || telemetrySingleton.PaTemp2 == null)
        {
            logger.LogWarning("Telemetry data not yet complete");
            return false;
        }

        if (lastChassisHumidity != telemetrySingleton.ChassisHumidity.Value || lastChassisTemp != telemetrySingleton.ChassisTemperature.Value || lastPaTemp1 != telemetrySingleton.PaTemp1.Value || lastPaTemp2 != telemetrySingleton.PaTemp2.Value)
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
