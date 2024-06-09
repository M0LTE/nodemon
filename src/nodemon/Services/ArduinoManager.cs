using Microsoft.Extensions.Options;
using nodemon.Configuration;
using System.IO.Ports;

namespace nodemon.Services;

public class ArduinoManager : IHostedService, IDisposable
{
    private readonly IOptions<NodeMonConfig> options;
    private readonly ILogger<ArduinoManager> logger;
    private readonly ArduinoSingleton arduino;
    private readonly SerialPort arduinoSerialPort;
    private bool stopping;

    public ArduinoManager(IOptions<NodeMonConfig> options, ILogger<ArduinoManager> logger, ArduinoSingleton arduino)
    {
        this.options = options;
        this.logger = logger;
        this.arduino = arduino;
        arduinoSerialPort = new(options.Value.ArduinoPort, 9600);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (options.Value?.ArduinoPort == null)
        {
            logger.LogWarning("No Arduino port configured");
            return;
        }

        logger.LogInformation("Opening serial port {port}", options.Value.ArduinoPort);

        arduinoSerialPort.Open();
        arduinoSerialPort.DiscardInBuffer();

        arduinoSerialPort.ReadTimeout = 1000;
        while (true)
        {
            arduinoSerialPort.Write("?");
            try
            {
                if (arduinoSerialPort.ReadLine() == "info: ok\r")
                {
                    logger.LogInformation("Found firmware running");
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("No response from Arduino sketch; is this the right serial port? Looking for a board running https://github.com/M0LTE/arduino-relay-control/blob/main/relay_control_and_temp_hum_sense/relay_control_and_temp_hum_sense.ino");
            }
            await Task.Delay(5000, cancellationToken);
        }
        arduinoSerialPort.ReadTimeout = 65000;

        arduino.OnSetRelay = (relay, state) =>
        {
            arduinoSerialPort.Write($"r{relay}{(state ? 1 : 0)}");
            logger.LogInformation("Commanding relay {relay} to {state}", relay, state);
        };

        var poweredAnyOn = false;
        foreach (var radioPort in options.Value.Ports)
        {
            if (radioPort.Skip)
            {
                continue;
            }

            if (radioPort.AutoPowerOn)
            {
                arduino.SetRelay(radioPort.RelayPin, true);
                poweredAnyOn = true;
            }
        }

        if (poweredAnyOn)
        {
            logger.LogInformation("Allowing time for radios to power on");
            await Task.Delay(5000, cancellationToken);
        }

        _ = Task.Run(() =>
        {
            while (!stopping)
            {
                string data;
                try
                {
                    data = arduinoSerialPort.ReadLine();
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("Arduino read timed out");
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error reading from Arduino");
                    return;
                }
                logger.LogInformation(data.Trim());
            }
        }, cancellationToken);

        logger.LogInformation("Arduino startup complete");
    }
   
    public Task StopAsync(CancellationToken cancellationToken)
    {
        stopping = true;
        return Task.CompletedTask;
    }

    public void Dispose() => arduinoSerialPort?.Dispose();
}
