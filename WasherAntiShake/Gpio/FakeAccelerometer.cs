using System.Numerics;

namespace Washer.Gpio
{
    public class FakeAccelerometer : IAccelerometer
    {
        public Vector3 Acceleration => new Vector3(0, 0, 0);

        public void Dispose()
        {
        }
    }
}