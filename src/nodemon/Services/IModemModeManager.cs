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
    public async Task<bool> SetModemMode(string port, int id)
    {
        var kernelInterface = options.Value.Ports.FirstOrDefault(p => p.Id == port)?.KernelInterface;

        if (kernelInterface == null)
        {
            //logger.LogWarning("No kernel interface configured for port {port}", port);
            logger.LogWarning("TODO: implement sending KISS SETHW command in non-kernel ports");
            return false;
        }

        var parameters = $"-p {kernelInterface} -h {id}";

        await RunAsync("/usr/sbin/kissparms", parameters); // +16 makes the setting transient, saving flash writes

        logger.LogInformation("Set modem mode for port {port} to {mode} (command: kissparms {})", port, id, parameters);
        return true;
    }
}