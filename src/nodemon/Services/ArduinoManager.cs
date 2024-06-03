using Microsoft.Extensions.Options;
using nodemon.Configuration;
using System.IO.Ports;

namespace nodemon.Services;

public class ArduinoManager(IOptions<NodeMonConfig> options, ILogger<ArduinoManager> logger, Arduino arduino) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (options.Value?.ArduinoPort == null)
        {
            logger.LogWarning("No Arduino port configured");
            return Task.CompletedTask;
        }

        _ = Task.Run(async () =>
        {

            while (true)
            {
                try
                {
                    using SerialPort port = new(options.Value.ArduinoPort, 9600);
                    port.Open();

                    arduino.OnSetRelay = (relay, state) =>
                    {
                        port.Write($"r{relay}{(state ? 1 : 0)}");
                        logger.LogInformation("Relay {relay} set to {state}", relay, state);
                    };

                    while (true)
                    {
                        var data = port.ReadLine();
                        logger.LogInformation(data);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error");
                }
                finally
                {
                    arduino.OnSetRelay = null;
                    await Task.Delay(10000, cancellationToken);
                }
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class Arduino
{
    public Action<int, bool>? OnSetRelay { get; set; }

    public void SetRelay(int relay, bool state)
    {
        OnSetRelay?.Invoke(relay, state);
    }
}