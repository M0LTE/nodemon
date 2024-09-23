
namespace nodemon.Services;

public class PersistentStateService
{
    private readonly string dir;

    public PersistentStateService(ILogger<PersistentStateService> logger)
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        dir = Path.Combine(appdata, ".nodemon");

        if (Directory.Exists(appdata) && !Directory.Exists(dir))
        {
            logger.LogInformation("Creating {dir} for persistent state", dir);
            Directory.CreateDirectory(dir);
        }
        else
        {
            logger.LogInformation("Using {dir} for persistent state", dir);
        }
    }

    internal int RestoreInt(string key, int defaultValue)
    {
        if (!File.Exists(Path.Combine(dir, key)))
        {
            return defaultValue;
        }

        if (!int.TryParse(File.ReadAllText(Path.Combine(dir, key)), out int value))
        {
            return defaultValue;
        }

        return value;
    }

    internal void SaveInt(string key, int value) => File.WriteAllText(Path.Combine(dir, key), value.ToString());
}
