using Microsoft.AspNetCore.Mvc;
using nodemon.Services;

namespace nodemon.Controllers;

[Route("api/node")]
[ApiController]
public class NodeApiController(ILogger<NodeApiController> logger, NodeService nodeService, ArduinoSingleton arduinoSingleton) : ControllerBase
{
    [HttpGet()]
    public async Task<IActionResult> GetNodeSoftwareState()
    {
        logger.LogInformation("Get node state");
        return Ok(new NodeState { Type = "bpq", BpqState = await nodeService.GetBpqState() });
    }

    [HttpPatch("restart")]
    public async Task<IActionResult> RestartNodeSoftware()
    {
        logger.LogInformation("Got request to restart node software");
        await nodeService.RestartNodeSoftware();
        return Ok();
    }

    [HttpPatch("start")]
    public async Task<IActionResult> StartNodeSoftware()
    {
        logger.LogInformation("Got request to start node software");
        return Ok();
    }

    [HttpPatch("stop")]
    public IActionResult StopNodeSoftware()
    {
        logger.LogInformation("Got request to stop node software");
        return Ok();
    }

    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NodeSoftwareConfig))]
    public IActionResult GetNodeConfig()
    {
        logger.LogInformation("Get node software config");
        return Ok("config file here");
    }

    [HttpPut("bpq/config")]
    public IActionResult SetBpqConfig([FromBody] string config)
    {
        logger.LogInformation("Set BPQ config, {length} bytes", config.Length);
        return Ok();
    }

    [HttpPatch("node/system/stop")]
    public IActionResult StopSystem()
    {
        logger.LogInformation("Got request to stop system");
        return Ok();
    }
}
