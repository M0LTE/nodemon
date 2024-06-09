namespace nodemon.Services;

public class ArduinoSingleton
{
    public Action<int, bool>? OnSetRelay { get; set; }

    public void SetRelay(int relay, bool state)
    {
        OnSetRelay?.Invoke(relay, state);
    }
}