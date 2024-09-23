namespace nodemon.Configuration;

public class NodeMonConfig
{
    public class Port
    {
        public required string Id { get; set; }
        public required int RelayPin { get; set; }
        public bool AutoPowerOn { get; set; }
        public required string RadioPort { get; set; }
        public required int RadioBaud { get; set; }
        public bool Skip { get; set; }
        /// <summary>
        /// As in /etc/axports
        /// </summary>
        public string? KernelAxport { get; set; }
        public List<Channel>? Channels { get; set; } = [];
        public int? PaTempTelemetryChannel { get; set; }
    }

    public required string ArduinoPort { get; set; }
    public required Port[] Ports { get; set; }

    public record Channel
    {
        public int Id { get; set; }
        public decimal MHz { get; set; }
        public int W { get; set; }
    }
}