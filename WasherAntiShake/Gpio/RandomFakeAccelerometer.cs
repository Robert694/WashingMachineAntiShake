using System;
using System.Numerics;

namespace Washer.Gpio
{
    public class RandomFakeAccelerometer : IAccelerometer
    {
        private readonly Random _rnd;
        public Vector3 Acceleration => new Vector3(_rnd.Next(0, 2), _rnd.Next(0,2), _rnd.Next(0, 2));

        public RandomFakeAccelerometer(Random rnd = null)
        {
            _rnd = rnd ?? new Random();
        }

        public void Dispose()
        {
        }
    }
}