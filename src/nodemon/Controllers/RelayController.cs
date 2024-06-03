using Microsoft.AspNetCore.Mvc;
using nodemon.Services;

namespace nodemon.Controllers;

[ApiController]
[Route("[controller]")]
public class RelayController(Arduino arduino) : ControllerBase
{
    [HttpPut(Name = "SetRelays")]
    public void SetRelays(string relays, bool state)
    {
        foreach (var relay in relays.Split(','))
        {
            if (int.TryParse(relay, out var r))
            {
                arduino.SetRelay(r, state);
            }
        }
    }
}
