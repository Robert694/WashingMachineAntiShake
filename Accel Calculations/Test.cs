using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Accel_Calculations
{
    public static class Test
    {
        private static Vector3 lowPass(Vector3 input, float alpha, Vector3 mean)
        {
            return new Vector3(
                input.X * alpha + mean.X * (1.0f - alpha),
                input.Y * alpha + mean.Y * (1.0f - alpha),
                input.Z * alpha + mean.Z * (1.0f - alpha));
        }

        public static void ProcessDataToMagMax(int windowSize)
        {
            StringBuilder sb = new StringBuilder();
            var magnitudes = new FixedSizedQueue<double>(windowSize);
            var deltas = new FixedSizedQueue<double>(windowSize * 2);
            //sb.AppendLine("Minimum,Average,Maximum,Delta,AvgDelta");
            sb.AppendLine("Magnitude,Delta,AvgDelta");
            foreach (var line in File.ReadAllLines("data.csv"))
            {
                try
                {
                    string[] split = line.Split(',');
                    var x = double.Parse(split[0]);
                    var y = double.Parse(split[1]);
                    var z = double.Parse(split[2]);
                    var magnitude = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
                    var magnitudeMinusOne = Math.Abs(magnitude - 1);
                    magnitudes.Enqueue(magnitude);
                    var avg = magnitudes.Average();
                    var min = magnitudes.Min();
                    var max = magnitudes.Max();
                    var delta = Math.Abs(max - min);
                    deltas.Enqueue(delta);
                    var avgdelta = deltas.Average();
                    int roundTo = 3;
                    //sb.AppendLine($"{Math.Round(magnitude, roundTo)},{Math.Round(min,roundTo)},{Math.Round(avg, roundTo)},{Math.Round(max, roundTo)},{Math.Round(delta, roundTo)},{Math.Round(avgdelta, roundTo)}");
                    //var step = 0.025;
                    //var steppedDelta = Math.Round(avgdelta / step, MidpointRounding.ToZero) * step;
                    sb.AppendLine($"{Math.Round(magnitude, roundTo)},{Math.Round(delta, roundTo)},{Math.Round(avgdelta, roundTo)}");
                }
                catch (Exception e)
                {
                }
            }
            File.WriteAllText("output_magnitude.csv", sb.ToString());
        }

        public static void ProcessDataToMagMax2(int windowSize)
        {
            StringBuilder sb = new StringBuilder();
            var magnitudes = new FixedSizedQueue<double>(windowSize);
            var minMagnitudes = new FixedSizedQueue<double>(windowSize * 4);
            var maxMagnitudes = new FixedSizedQueue<double>(windowSize * 4);
            var deltas = new FixedSizedQueue<double>(windowSize);
            //sb.AppendLine("Minimum,Average,Maximum,Delta,AvgDelta");
            sb.AppendLine("Magnitude,Min,Max,Delta,AvgDelta");
            int count = 0;
            foreach (var line in File.ReadAllLines("data.csv"))
            {
                try
                {
                    string[] split = line.Split(',');
                    var x = double.Parse(split[0]);
                    var y = double.Parse(split[1]);
                    var z = double.Parse(split[2]);
                    var magnitude = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
                    var magnitudeMinusOne = Math.Abs(magnitude - 1);
                    magnitudes.Enqueue(magnitude);
                    var min = magnitudes.Min();
                    var max = magnitudes.Max();
                    minMagnitudes.Enqueue(min);
                    maxMagnitudes.Enqueue(max);
                    var minMagsAvg = minMagnitudes.Average();
                    var maxMagsAvg = maxMagnitudes.Average();
                    var delta = Math.Abs(maxMagsAvg - minMagsAvg);
                    deltas.Enqueue(delta);
                    var avgdelta = deltas.Average();
                    int roundTo = 3;
                    sb.AppendLine($"{Math.Round(magnitude, roundTo)},{Math.Round(minMagsAvg, roundTo)},{Math.Round(maxMagsAvg, roundTo)},{Math.Round(delta, roundTo)},{Math.Round(avgdelta, roundTo)}");
                }
                catch (Exception e)
                {
                }
            }
            File.WriteAllText("output_magnitude.csv", sb.ToString());
        }


        public static void ProcessDataToMagMax3(int windowSize, double filtered_coeff = 0.05)
        {
            StringBuilder sb = new StringBuilder();
            var magnitudes = new FixedSizedQueue<double>(windowSize);

            var deltas = new FixedSizedQueue<double>(windowSize);
            //sb.AppendLine("Minimum,Average,Maximum,Delta,AvgDelta");

            double min_filtered_shake = 0;
            double max_filtered_shake = 0;

            sb.AppendLine("Magnitude,Min,Max,Delta,AvgDelta");
            int count = 0;
            foreach (var line in File.ReadAllLines("data.csv"))
            {
                try
                {
                    string[] split = line.Split(',');
                    var x = double.Parse(split[0]);
                    var y = double.Parse(split[1]);
                    var z = double.Parse(split[2]);
                    var magnitude = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
                    var magnitudeMinusOne = Math.Abs(magnitude - 1);
                    magnitudes.Enqueue(magnitude);
                    var min = magnitudes.Min();
                    var max = magnitudes.Max();
                    min_filtered_shake = (min * filtered_coeff) + (min_filtered_shake * (1 - filtered_coeff));
                    max_filtered_shake = (max * filtered_coeff) + (max_filtered_shake * (1 - filtered_coeff));
                    var delta = Math.Abs(max_filtered_shake - min_filtered_shake);
                    deltas.Enqueue(delta);
                    var avgdelta = deltas.Average();
                    int roundTo = 3;
                    sb.AppendLine($"{Math.Round(magnitude, roundTo)},{Math.Round(min_filtered_shake, roundTo)},{Math.Round(max_filtered_shake, roundTo)},{Math.Round(delta, roundTo)},{Math.Round(avgdelta, roundTo)}");
                }
                catch (Exception e)
                {
                }
            }
            File.WriteAllText("output_magnitude.csv", sb.ToString());
        }

        public static void ProcessDataFilter()
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            double filtered_shake = 0;
            double filtered_coeff = 0.05;
            foreach (var line in File.ReadAllLines("data.csv"))
            {
                try
                {
                    string[] split = line.Split(',');
                    var x = double.Parse(split[0]);
                    var y = double.Parse(split[1]);
                    var z = double.Parse(split[2]);
                    var mag = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
                    var magnitude = mag;

                    magnitude -= 1;
                    magnitude *= magnitude;
                    filtered_shake = (magnitude * filtered_coeff) + (filtered_shake * (1 - filtered_coeff));
                    sb.AppendLine($"{mag},{Math.Round(filtered_shake*(1 - filtered_coeff)*100, 4)}");
                }
                catch (Exception e)
                {
                }
            }
            File.WriteAllText("output_magnitude.csv", sb.ToString());
        }


        public static void ProcessDataToMagnitude(int windowSize)
        {
            StringBuilder sb = new StringBuilder();
            double rolingavg = 0;
            var avgValues = new FixedSizedQueue<double>(windowSize);
            foreach (var line in File.ReadAllLines("data.csv"))
            {
                try
                {
                    string[] split = line.Split(',');
                    var x = double.Parse(split[0]);
                    var y = double.Parse(split[1]);
                    var z = double.Parse(split[2]);
                    //Vector3 vec = new Vector3((float)x, (float)y, (float)z);
                    var magnitude = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
                    magnitude = Math.Abs(magnitude - 1);
                    avgValues.Enqueue(magnitude);
                    var avg = avgValues.Average();
                    rolingavg = ApproxRollingAverage(rolingavg, magnitude, windowSize);

                    //var step = 0.02;
                    //v = Math.Round(v / step) * step;
                    sb.AppendLine($"{magnitude},{avg},{rolingavg}");
                }
                catch (Exception e)
                {
                }
            }
            File.WriteAllText("output_magnitude.csv", sb.ToString());
        }

        public static void ProcessNoiseData()
        {
            //List<Vector3> vectors = new List<Vector3>();
            //foreach (var line in File.ReadAllLines("data.csv"))
            //{
            //    try
            //    {
            //        string[] split = line.Split(',');
            //        var x = double.Parse(split[0]);
            //        var y = double.Parse(split[1]);
            //        var z = double.Parse(split[2]);
            //        vectors.Add(new Vector3((float) x, (float)y, (float)z));
            //
            //    }
            //    catch (Exception e)
            //    {
            //    }
            //}
            //Console.WriteLine($"{vectors.Average(x => x.X)},{vectors.Average(x => x.Y)},{vectors.Average(x => x.Z)}");
            //return;

            StringBuilder sb = new StringBuilder();
            var denoised = new List<float>();
            var mean = new Vector3(.1f, .1f, .1f);
            sb.AppendLine($"{string.Join(",", "X", "Y", "Z")}");
            KalmanFilter filterX = null;
            KalmanFilter filterY = null;
            KalmanFilter filterZ = null;
            foreach (var line in File.ReadAllLines("noise.csv"))
            {
                try
                {
                    string[] split = line.Split(',');
                    var x = double.Parse(split[0]);
                    var y = double.Parse(split[1]);
                    var z = double.Parse(split[2]);
                    var v = new Vector3((float)x, (float)y, (float)z);

                    if (filterX == null || filterY == null || filterZ == null)
                    {
                        filterX = new KalmanFilter(1, 1, 0.125, 1, 0.1, v.X);
                        filterY = new KalmanFilter(1, 1, 0.125, 1, 0.1, v.Y);
                        filterZ = new KalmanFilter(1, 1, 0.125, 1, 0.1, v.Z);
                    }
                    sb.AppendLine($"{Math.Round(filterX.Output(v.X), 2)},{Math.Round(filterY.Output(v.Y), 2)},{Math.Round(filterZ.Output(v.Z), 2)}");
                    denoised.Clear();
                }
                catch (Exception e)
                {
                }
            }
            File.WriteAllText("output_denoise.csv", sb.ToString());
        }

        public static void GetData(List<Vector3> data, Func<Vector3, float> getProp)
        {
            if (data == null || data.Count <= 0) return;
            var min = data.Min(getProp);
            var max = data.Max(getProp);
            var avg = data.Average(getProp);
            Console.WriteLine($"-{Math.Abs(min - avg)} +{Math.Abs(max - avg)}");
        }

        public static double ApproxRollingAverage(double avg, double new_sample, int N)
        {

            avg -= avg / N;
            avg += new_sample / N;
            return avg;
        }
    }
}