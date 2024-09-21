using CliWrap;
using Microsoft.Extensions.Options;
using nodemon.Configuration;

namespace nodemon.Services;

public class MonitorService(IOptions<NodeMonConfig> config, ILogger<MonitorService> logger) : IHostedService
{
    private readonly List<CancellationTokenSource> cancellationTokenSources = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var port in config.Value!.Ports.Where(p => !p.Skip))
        {
            logger.LogInformation("Starting monitor for port {port}", port.Id);

            var cts = new CancellationTokenSource();
            cancellationTokenSources.Add(cts);

            Cli.Wrap($"/usr/bin/axlisten -p {port.KernelAxport}")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(data => logger.LogInformation(data)))
                .ExecuteAsync(cts.Token);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSources.ForEach(cts => cts.Cancel());
        return Task.CompletedTask;
    }
}
