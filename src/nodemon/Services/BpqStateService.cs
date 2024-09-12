using SimpleExec;
using static SimpleExec.Command;

namespace nodemon.Services;

public interface INodeSoftwareStateService
{
    Task<bool> IsBinaryPresent();
    Task<bool> IsServiceInstalled();
    Task<bool> ISserviceEnabled();
    Task<bool> IsServiceRunning();
    Task<bool> IsConfigPresent();
}

public class DevNodeSoftwareStateService : INodeSoftwareStateService
{
    private static Random random = new();
    public Task<bool> IsBinaryPresent()
    {
        return Task.FromResult(random.NextDouble() < 0.5);
    }

    public Task<bool> IsServiceInstalled()
    {
        return Task.FromResult(true);
    }
    public Task<bool> ISserviceEnabled()
    {
        return Task.FromResult(true);
    }

    public Task<bool> IsServiceRunning()
    {
        return Task.FromResult(false);
    }

    public Task<bool> IsConfigPresent()
    {
        return Task.FromResult(true);
    }
}

public class LinuxBpqStateService : INodeSoftwareStateService
{
    public Task<bool> IsBinaryPresent()
    {
        return Task.FromResult(File.Exists("/opt/linbpq/linbpq"));
    }

    public async Task<bool> IsServiceRunning()
    {
        try
        {
            await RunAsync("systemctl", "status linbpq");
            return true;
        }
        catch (ExitCodeException)
        {
            return false;
        }
    }

    public async Task<bool> ISserviceEnabled()
    {
        try
        {
            var (stdout, stderr) = await ReadAsync("systemctl", "status linbpq");
            return stdout.Contains("linbpq.service; enabled;");
        }
        catch (ExitCodeReadException ex)
        {
            if (ex.ExitCode == 3 && ex.StandardOutput.Contains("linbpq.service; enabled;"))
            {
                return true;
            }

            return false;
        }
    }

    public async Task<bool> IsServiceInstalled()
    {
        try
        {
            await RunAsync("systemctl", "status linbpq");
            return true;
        }
        catch (ExitCodeException ex)
        {
            if (ex.ExitCode == 3)
            {
                return true;
            }

            return false;
        }
    }

    public Task<bool> IsConfigPresent()
    {
        return Task.FromResult(File.Exists("/etc/bpq32.cfg"));
    }
}