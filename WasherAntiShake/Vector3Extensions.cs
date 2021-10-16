using System;
using System.Numerics;

namespace Washer
{
    public static class Vector3Extensions
    {
        public static double Magnitude(this Vector3 vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2) + Math.Pow(vector.Y, 2) + Math.Pow(vector.Z, 2));
        }
    }
}