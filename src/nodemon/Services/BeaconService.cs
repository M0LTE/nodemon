using CliWrap;
using CliWrap.Buffered;
using nodemon.Models;

namespace nodemon.Services;

public class BeaconService
{
    public async Task<bool> Send(CallSsid source, CallSsid dest, string axPortName, string payload)
    {
        var result = await Cli.Wrap("/usr/sbin/beacon")
                .WithArguments($"-s -c \"{source}\" -d \"{dest}\" {axPortName} \"{payload}\"")
                .ExecuteBufferedAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);

        return result.IsSuccess;
    }
}

public class BeaconOnStartup(BeaconService beaconService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await beaconService.Send("M0LTE-2", "APRS", "2m", ">nodemon");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}