namespace nodemon.Services;

public class ArduinoSingleton
{
    public Action<int, bool>? OnSetRelay { get; set; }
    public Func<int, bool>? OnGetRelay { get; set; }

    public void SetRelay(int relay, bool state) => OnSetRelay?.Invoke(relay, state);

    public bool GetRelay(int relay) => OnGetRelay?.Invoke(relay) ?? false;
}