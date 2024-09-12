using Microsoft.Extensions.Options;
using nodemon.Configuration;

namespace nodemon.Services;

public class NodeService(
    ArduinoSingleton arduino, 
    ILogger<NodeService> logger, 
    INodeSoftwareStateService bpqStateService,
    IOptions<NodeMonConfig> options,
    TaitSingleton taitSingleton
    )
{
    public PortsResponse GetPorts()
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceState> GetBpqState()
    {
        var result = new ServiceState
        {
            Present = await bpqStateService.IsBinaryPresent(),
            Installed = await bpqStateService.IsServiceInstalled(),
            Enabled = await bpqStateService.ISserviceEnabled(),
            ConfigPresent = await bpqStateService.IsConfigPresent(),
            Running = await bpqStateService.IsServiceRunning(),
        };
        
        return result;
    }

    public Task SetRadioChannel(string port, int channel)
    {
        taitSingleton.Radios[port].GoToChannel(channel);
        return Task.CompletedTask;
    }

    internal Task<int> GetRadioChannel(string port)
    {
        return Task.FromResult(taitSingleton.Radios[port].GetCurrentChannel());
    }

    internal async Task RestartNodeSoftware()
    {
        throw new NotImplementedException();
    }
}

public record ServiceState
{
    public bool Installed { get; set; }
    public bool Running { get; set; }
    public bool Present { get; set; }
    public bool Enabled { get; set; }
    public bool ConfigPresent { get; set; }
}


public class PortsResponse
{
    public List<Port> Ports { get; set; } = [];
}

public class Port
{
    public required string Id { get; set; }
    public bool Running { get; set; }
    public double? PaTemp { get; set; }
    public double? Swr { get; set; }
    public double? NoiseFloor { get; set; }
    public DateTime? LastTx { get; set; }
    public DateTime? LastRx { get; set; }
}

public class NodeSoftwareConfig
{
    public string? BpqConfig { get; set; }
}