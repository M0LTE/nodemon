namespace nodemon.Services;

public class TelemetrySingleton
{
    public int? ChassisTemperature { get; set; }
    public int? ChassisHumidity { get; set; }
    public Dictionary<string, double> PaTemperatures { get; private set; } = [];
}
