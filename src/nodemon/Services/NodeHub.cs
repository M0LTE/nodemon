using Microsoft.AspNetCore.SignalR;

namespace nodemon.Services;

public class NodeHub : Hub
{
    private readonly ILogger<NodeHub> logger;
    private readonly ArduinoSingleton arduinoSingleton;

    public NodeHub(ILogger<NodeHub> logger, ArduinoSingleton arduinoSingleton)
    {
        this.logger = logger;
        this.arduinoSingleton = arduinoSingleton;
    }


    public Task ToggleChanged(int relay, bool isOn)
    {
        logger.LogInformation("ToggleChanged: {relay} {isOn}", relay, isOn);
        arduinoSingleton.SetRelay(relay, isOn);

        return Task.CompletedTask;
    }
}
