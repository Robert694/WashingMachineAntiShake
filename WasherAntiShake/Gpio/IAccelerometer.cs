using System;
using System.Numerics;

namespace Washer.Gpio
{
    public interface IAccelerometer : IDisposable
    {
        public Vector3 Acceleration { get; }
    }
}