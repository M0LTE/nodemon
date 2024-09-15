using Microsoft.Extensions.Options;
using nodemon.Configuration;
using static SimpleExec.Command;

namespace nodemon.Services;

public interface IModemModeManager
{
    Task<bool> SetModemMode(string port, int id);
}

public class ModemModeManager(ILogger<ModemModeManager> logger, IOptions<NodeMonConfig> options) : IModemModeManager
{
    public async Task<bool> SetModemMode(string port, int ninoModeId)
    {
        var kernelAxport = options.Value.Ports.FirstOrDefault(p => p.Id == port)?.KernelAxport;

        if (kernelAxport == null)
        {
            //logger.LogWarning("No kernel interface configured for port {port}", port);
            logger.LogWarning("TODO: implement sending KISS SETHW command in non-kernel ports");
            return false;
        }

        var parameters = $"-p {kernelAxport} -h {ninoModeId + 16}"; // +16 makes the setting transient, saving flash writes

        await RunAsync("/usr/sbin/kissparms", parameters);

        logger.LogInformation("Set modem mode for port {port} to {mode} (command: kissparms {parameters})", port, ninoModeId, parameters);
        return true;
    }
}