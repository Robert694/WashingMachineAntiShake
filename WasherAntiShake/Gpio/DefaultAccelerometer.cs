using System.Device.Spi;
using System.Numerics;
using Iot.Device.Adxl345;

namespace Washer.Gpio
{
    public class DefaultAccelerometer : IAccelerometer
    {
        private Adxl345 sensor;

        public Vector3 Acceleration => sensor.Acceleration;

        public DefaultAccelerometer(GravityRange range = GravityRange.Range04)
        {
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = Adxl345.SpiClockFrequency,
                Mode = Adxl345.SpiMode
            };
            var device = SpiDevice.Create(settings);
            //device.TransferFullDuplex(); -< read write registers using this
            // Set gravity measurement range ±4G
            sensor = new Adxl345(device, range);
        }

        public void Dispose()
        {
            sensor?.Dispose();
        }
    }
}