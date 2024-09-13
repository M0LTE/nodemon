using Microsoft.AspNetCore.SignalR;

namespace nodemon.Services;

public class NodeHub : Hub
{
    private readonly ILogger<NodeHub> logger;
    private readonly ArduinoSingleton arduinoSingleton;
    private readonly TaitSingleton taitSingleton;

    public NodeHub(ILogger<NodeHub> logger, ArduinoSingleton arduinoSingleton, TaitSingleton taitSingleton)
    {
        this.logger = logger;
        this.arduinoSingleton = arduinoSingleton;
        this.taitSingleton = taitSingleton;
    }

    // these are methods which get called by the UI, from Javascript

    public Task ToggleChanged(int relay, bool isOn)
    {
        logger.LogInformation("ToggleChanged: {relay} {isOn}", relay, isOn);
        arduinoSingleton.SetRelay(relay, isOn);

        return Task.CompletedTask;
    }

    public Task ChannelChanged(string portId, string channel)
    {
        if (taitSingleton.Radios.TryGetValue(portId, out var radio))
        {
            radio.GoToChannel(int.Parse(channel));
        }

        return Task.CompletedTask;
    }

    public Task<int> GetChannel(string portId)
    {
        if (taitSingleton.Radios.TryGetValue(portId, out var radio))
        {
            return Task.FromResult(radio.GetCurrentChannel());
        }

        return Task.FromResult(-1);
    }
}
