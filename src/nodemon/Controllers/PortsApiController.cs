using Microsoft.AspNetCore.Mvc;
using nodemon.Services;

namespace nodemon.Controllers;

[Route("api/ports")]
[ApiController]
public class PortsApiController(ILogger<PortsApiController> logger, NodeService nodeService, ArduinoSingleton arduinoSingleton) : ControllerBase
{
    [HttpPost("{port}/transmit")]
    public IActionResult Transmit(string port, [FromBody] byte[] bytes)
    {
        logger.LogInformation("Got request to transmit {count} bytes on port {port}", bytes.Length, port);
        return Ok();
    }

    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PortsResponse))]
    public IActionResult GetPorts()
    {
        logger.LogInformation("GetPorts() called");
        return Ok(nodeService.GetPorts());
    }

    [HttpGet("{port}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Port))]
    public IActionResult GetPortInfo(string port)
    {
        logger.LogInformation("Get port {port}", port);
        return Ok();
    }

    [HttpPost("{port}/start")]
    public IActionResult StartPort(string port)
    {
        logger.LogInformation("Start port {port}", port);
        return Ok();
    }

    [HttpPost("{port}/stop")]
    public IActionResult StopPort(string port)
    {
        logger.LogInformation("Stop port {port}", port);
        return Ok();
    }

    [HttpGet("{port}/radio/channel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public async Task<IActionResult> GetRadioChannel(string port)
    {
        logger.LogInformation("Get current radio channel for port {port}", port);
        return Ok(await nodeService.GetRadioChannel(port));
    }

    [HttpPut("{port}/radio/channel")]
    public async Task<IActionResult> SetRadioChannel(string port, int value)
    {
        logger.LogInformation("Set radio channel for port {port} to {channel}", port, value);
        await nodeService.SetRadioChannel(port, value);
        return Ok();
    }

    [HttpGet("{port}/modem/mode")]
    public IActionResult GetModemMode(string port)
    {
        logger.LogInformation("Get current modem mode for port {port}", port);
        return Ok("mode " + port);
    }

    [HttpPut("{port}/modem/mode")]
    public IActionResult SetModemMode(string port, string mode)
    {
        logger.LogInformation("Set modem mode for port {port} to {mode}", port, mode);
        return Ok();
    }

    [HttpGet("{port}/radio/dcpower")]
    public IActionResult GetRadioDcPowerState(string port)
    {
        logger.LogInformation("Get current DCpower state for port {port}", port);
        return Ok("on");
    }

    [HttpPut("{port}/radio/dcpower")]
    public IActionResult SetRadioDcPowerState(string port, bool state)
    {
        logger.LogInformation("Set DC power state for port {port} to {state}", port, state);
        return Ok();
    }
}

public class NodeState
{
    public ServiceState? BpqState { get; set; }
    public required string Type { get; set; }
}

public class OneWireResult
{
    public List<Ds18b20>? Ds18B20s { get; set; } = [];
}

public class Ds18b20
{
    public required string Id { get; set; }
    public required double Temperature { get; set; }
}