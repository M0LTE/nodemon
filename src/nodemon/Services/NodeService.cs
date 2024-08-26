using Microsoft.Extensions.Options;
using nodemon.Configuration;

namespace nodemon.Services;

public class NodeService(
    ArduinoSingleton arduino, 
    ILogger<NodeService> logger, 
    IBpqStateService bpqStateService,
    IOptions<NodeMonConfig> options,
    TaitManager taitManager
    )
{
    public PortsResponse GetPorts()
    {
        throw new NotImplementedException();
    }

    public async Task<BpqState> GetBpqState()
    {
        var result = new BpqState
        {
            Present = await bpqStateService.IsBinaryPresent(),
            Installed = await bpqStateService.IsBpqServiceInstalled(),
            Enabled = await bpqStateService.IsBpqServiceEnabled(),
            ConfigPresent = await bpqStateService.IsConfigPresent(),
            Running = await bpqStateService.IsBpqServiceRunning(),
        };
        
        return result;
    }

    public async Task SetRadioChannel(string port, int channel)
    {
        await taitManager.SetChannel(port, channel);
    }
}

public record BpqState
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