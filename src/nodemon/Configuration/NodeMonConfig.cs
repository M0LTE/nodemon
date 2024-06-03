namespace nodemon.Configuration;

public class NodeMonConfig
{
    public class Port
    {
        public required string Id { get; set; }
        public required int RelayPin { get; set; }
        public required string RadioPort { get; set; }
        public required int RadioBaud { get; set; }
    }

    public required string ArduinoPort { get; set; }
    public required Port[] Ports { get; set; }
}
