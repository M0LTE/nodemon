using CliWrap;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using nodemon.Configuration;
using static nodemon.Configuration.NodeMonConfig;

namespace nodemon.Services;

public class KernelMonitorService(IHubContext<NodeHub> hubContext, IOptions<NodeMonConfig> config, ILogger<KernelMonitorService> logger) : IHostedService
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
                .WithStandardOutputPipe(PipeTarget.ToDelegate(async data => await Log(port, data)))
                .ExecuteAsync(cts.Token);
        }

        return Task.CompletedTask;
    }
    private async Task Log(NodeMonConfig.Port port, string data)
    {
        logger.LogInformation(data);
        await hubContext.Clients.All.SendAsync("RssiUpdate", new { port = port.Id, data });
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSources.ForEach(cts => cts.Cancel());
        return Task.CompletedTask;
    }
}
