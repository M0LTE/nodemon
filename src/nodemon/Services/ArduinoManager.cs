using Microsoft.Extensions.Options;
using nodemon.Configuration;
using System.IO.Ports;

namespace nodemon.Services;

public class ArduinoManager(IOptions<NodeMonConfig> options, ILogger<ArduinoManager> logger, ArduinoSingleton arduino, TelemetrySingleton telemetrySingleton) : IHostedService, IDisposable
{
    private readonly SerialPort arduinoSerialPort = new(options.Value.ArduinoPort, 9600);
    private bool stopping;
    private readonly Dictionary<int, bool> states = [];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (options.Value?.ArduinoPort == null)
        {
            logger.LogWarning("No Arduino port configured");
            return;
        }

        logger.LogInformation("Opening serial port {port}", options.Value.ArduinoPort);

        try
        {
            arduinoSerialPort.Open();
        }
        catch (FileNotFoundException)
        {
            logger.LogError("Serial port {port} does not exist", options.Value.ArduinoPort);
            return;
        }
        catch (Exception ex)
        {
            logger.LogError("Could not open serial port {port}: {message}", options.Value.ArduinoPort, ex.Message);
            return;
        }

        arduinoSerialPort.DiscardInBuffer();

        arduinoSerialPort.ReadTimeout = 1000;
        await Task.Delay(1000);
        while (true)
        {
            logger.LogInformation("Querying for running firmware");
            arduinoSerialPort.Write("?");
            try
            {
                if (arduinoSerialPort.ReadLine() == "info: ok\r")
                {
                    logger.LogInformation("Found firmware running");
                    break;
                }
            }
            catch (TimeoutException)
            {
                logger.LogWarning("No response from Arduino sketch; is this the right serial port? Looking for a board running https://github.com/M0LTE/arduino-relay-control/blob/main/relay_control_and_temp_hum_sense/relay_control_and_temp_hum_sense.ino");
            }
            await Task.Delay(1000, cancellationToken);
        }
        arduinoSerialPort.ReadTimeout = 65000;

        arduino.OnSetRelay = (relay, state) =>
        {
            states[relay] = state;
            arduinoSerialPort.Write($"r{relay}{(state ? 1 : 0)}");
            logger.LogInformation("Commanding relay {relay} to {state}", relay, state);
        };

        arduino.OnGetRelay = (relay) =>
        {
            return states[relay];
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
                catch (ObjectDisposedException)
                {
                    // shutting down
                    return;
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

                // sensor: 22C 76%
                if (data.StartsWith("sensor: "))
                {
                    HandleSensorData(data);
                }
            }
        }, cancellationToken);

        logger.LogInformation("Arduino startup complete");
    }

    private void HandleSensorData(string data)
    {
        // sensor: 22C 76%
        
        var parts = data.Split(' ');
        if (parts.Length != 3)
        {
            logger.LogWarning("Invalid sensor data: {data}", data);
            return;
        }

        if (int.TryParse(parts[1][..^1], out var temperature))
        {
            telemetrySingleton.ChassisTemperature = temperature;
        }

        if (int.TryParse(parts[2][..^1], out var humidity))
        {
            telemetrySingleton.ChassisHumidity = humidity;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        stopping = true;
        return Task.CompletedTask;
    }

    public void Dispose() => arduinoSerialPort?.Dispose();
}
