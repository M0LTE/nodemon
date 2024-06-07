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

                    await Task.Delay(5000);
                    foreach (var radioPort in options.Value.Ports)
                    {
                        if (radioPort.AutoPowerOn)
                        {
                            arduino.SetRelay(radioPort.RelayPin, true);
                            logger.LogInformation("Relay {relay} auto power on", radioPort.RelayPin);
                        }
                    }

                    if (options.Value.Ports.Length > 0)
                    {
                        await Task.Delay(5000);
                    }

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