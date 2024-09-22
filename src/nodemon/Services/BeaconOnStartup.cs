namespace nodemon.Services;

public class BeaconOnStartup(BeaconService beaconService) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await beaconService.SendAprsStatus("M0LTE-2", "APRS", "2m", "nodemon startup");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
