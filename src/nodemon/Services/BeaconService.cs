using CliWrap;
using CliWrap.Buffered;
using nodemon.Models;

namespace nodemon.Services;

public class BeaconService(ILogger<BeaconService> logger)
{
    private int? telemetryChannels;

    public async Task<bool> SendUI(CallSsid source, CallSsid dest, string axPortName, string payload)
    {
        logger.LogInformation("Sending beacon from {source} to {dest} on {axPortName}: {payload}", source, dest, axPortName, payload);

        var result = await Cli.Wrap("/usr/sbin/beacon")
                .WithArguments($"-s -c \"{source}\" -d \"{dest}\" {axPortName} \"{payload}\"")
                .ExecuteBufferedAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        return result.IsSuccess;
    }

    public Task<bool> SendAprsStatus(CallSsid source, CallSsid dest, string axPortName, string status)
        => SendUI(source, dest, axPortName, $">{status}");

    // https://github.com/wb2osz/direwolf/blob/master/doc/APRS-Telemetry-Toolkit.pdf

    /// <summary>
    /// Send structured data as APRS telemetry values
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <param name="axPortName"></param>
    /// <param name="seq"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Task<bool> SendAprsTelemetryValues(CallSsid source, CallSsid dest, string axPortName, int seq, params int[] values)
    {
        if (seq < 0)
        {
            throw new ArgumentException("Invalid sequence number");
        }

        seq %= 1000; // rolls over to 0 after 999

        if (values.Length == 0 || values.Length > 8)
        {
            throw new ArgumentException("Invalid number of telemetry values");
        }

        CheckChannelCount(values);

        var payload = $"T#{seq},{string.Join(",", values)}";
        return SendUI(source, dest, axPortName, payload);
    }

    public Task<bool> SendAprsTelemetrySeriesLabels(CallSsid source, CallSsid dest, string axPortName, params string[] labels)
        => SendAprsTelemetryMeta(source, dest, axPortName, "PARM", labels);

    public Task<bool> SendAprsTelemetrySeriesUnits(CallSsid source, CallSsid dest, string axPortName, params string[] labels)
        => SendAprsTelemetryMeta(source, dest, axPortName, "UNIT", labels);

    private Task<bool> SendAprsTelemetryMeta(CallSsid source, CallSsid dest, string axPortName, string meta, params string[] labels)
    {
        if (labels.Length == 0 || labels.Length > 8)
        {
            throw new ArgumentException("Invalid number of telemetry labels");
        }

        CheckChannelCount(labels);

        // :M0LTE-2  :PARM.Rabbits,Rats,Voles
        var payload = $":{source,-9}:{meta}.{string.Join(",", labels)}";
        return SendUI(source, dest, axPortName, payload);
    }

    private void CheckChannelCount(Array values)
    {
        if (telemetryChannels == null)
        {
            telemetryChannels = values.Length;
        }
        else if (values.Length != telemetryChannels)
        {
            logger.LogWarning("Unexpected number of telemetry values - previously we've been told we have {expected} channels, but you've send {actual} values", telemetryChannels, values.Length);
        }
    }
}
