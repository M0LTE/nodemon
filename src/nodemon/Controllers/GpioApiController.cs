using Microsoft.AspNetCore.Mvc;
using nodemon.Services;

namespace nodemon.Controllers;

[Route("api/gpio")]
[ApiController]
public class GpioApiController(ILogger<PortsApiController> logger, ArduinoSingleton arduinoSingleton) : ControllerBase
{

    [HttpGet("1wire")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OneWireResult))]
    public IActionResult Get1WireDevices()
    {
        logger.LogInformation("Get 1-wire devices");
        return Ok(new OneWireResult { Ds18B20s = [new Ds18b20 { Id = "aaa", Temperature = 20.1 }] });
    }

    [HttpPatch("relay/{relay}")]
    public IActionResult SetRelayState(int relay, [FromBody] bool state)
    {
        logger.LogInformation("Set relay {relay} state to {state}", relay, state);
        arduinoSingleton.SetRelay(relay, state);
        return Ok();
    }

    [HttpGet("relay/{relay}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public IActionResult GetRelayState(int relay)
    {
        logger.LogInformation("Get relay {relay} state", relay);
        return Ok(arduinoSingleton.GetRelay(relay));
    }
}
