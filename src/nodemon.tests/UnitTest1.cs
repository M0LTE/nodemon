namespace nodemon.tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var data = "sensor: 22C 76%";

            Assert.True(TryParseSensorData(data, out int temp, out int hum));
            Assert.Equal(22, temp);
            Assert.Equal(76, hum);
        }

        private static bool TryParseSensorData(string data, out int temp, out int hum)
        {
            var parts = data.Split(' ');
            if (parts.Length != 3)
            {
                temp = hum = default;
                return false;
            }

            if (int.TryParse(parts[1][..^1], out var temperature) && int.TryParse(parts[2][..^1], out var humidity))
            {
                temp = temperature;
                hum = humidity;
                return true;
            }

            temp = hum = default;
            return false;
        }
    }
}