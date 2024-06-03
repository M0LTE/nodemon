using Microsoft.Extensions.Options;
using nodemon.Configuration;

namespace nodemon.Services;

public class TaitMonitor(IOptions<NodeMonConfig> config, ILogger<TaitMonitor> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
