using nodemon.Services;

namespace nodemon.tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var data = " sensor: 22C 76% ";

            Assert.True(ArduinoManager.TryParseSensorData(data, out int temp, out int hum));
            Assert.Equal(22, temp);
            Assert.Equal(76, hum);
        }
    }
}