using CliWrap;
using Microsoft.Extensions.Options;
using nodemon.Configuration;

namespace nodemon.Services;

public class KernelMonitorService(IOptions<NodeMonConfig> config, ILogger<KernelMonitorService> logger) : IHostedService
{
    private readonly List<CancellationTokenSource> cancellationTokenSources = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var port in config.Value!.Ports.Where(p => !p.Skip && !string.IsNullOrWhiteSpace(p.KernelAxport)))
        {
            logger.LogInformation("Starting monitor for port {port}", port.Id);

            var cts = new CancellationTokenSource();
            cancellationTokenSources.Add(cts);

            Cli.Wrap("/usr/bin/axlisten")
                .WithArguments($"-p {port.KernelAxport}")
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
