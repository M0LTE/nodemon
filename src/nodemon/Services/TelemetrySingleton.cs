namespace nodemon.Services;

public class TelemetrySingleton
{
    public double? ChassisTemperature { get; set; }
    public double? ChassisHumidity { get; set; }
    public Dictionary<string, double> PaTemperatures { get; private set; } = [];
}
