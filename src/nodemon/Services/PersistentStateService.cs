
namespace nodemon.Services;

public class PersistentStateService
{
    private const string dir = "~/.nodemon";

    static PersistentStateService()
    {
        if (Directory.Exists("~/") && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    internal static int RestoreInt(string key, int defaultValue)
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

    internal static void SaveInt(string key, int value) => File.WriteAllText(Path.Combine(dir, key), value.ToString());
}
