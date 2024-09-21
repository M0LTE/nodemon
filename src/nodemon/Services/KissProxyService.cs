using kissproxylib;
using Microsoft.Extensions.Options;
using nodemon.Configuration;

namespace nodemon.Services;

public class KissProxyService(IOptions<NodeMonConfig> config, ILogger<KissProxyService> logger) : IHostedService
{
    private readonly List<Task> tasks = [];
    private readonly List<CancellationTokenSource> cts = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var port in config.Value.Ports.Where(p => !p.Skip && p.EnableProxy))
        {
            if (string.IsNullOrWhiteSpace(port.ModemPath) || !File.Exists(port.ModemPath))
            {
                logger.LogWarning("Invalid modem path configured for {port}", port.Id);
                continue;
            }

            if (!port.ModemBaud.HasValue)
            {
                logger.LogWarning("Invalid modem baud configured for {port}", port.Id);
                continue;
            }

            if (!port.ListenOnTcpPort.HasValue)
            {
                logger.LogWarning("Invalid TCP port configured for {port}", port.Id);
                continue;
            }

            if (config.Value.Ports.Any(p => p.ListenOnTcpPort == port.ListenOnTcpPort && p.Id != port.Id))
            {
                logger.LogWarning("TCP port {port} already in use", port.ListenOnTcpPort);
                continue;
            }

            var proxy = new KissProxy(logger);
            Task t = proxy.Run(port.ModemPath, port.ModemBaud.Value, port.ListenOnTcpPort.Value);
            tasks.Add(t);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.ForEach(ct => ct.Cancel());
        return Task.CompletedTask;
    }
}
