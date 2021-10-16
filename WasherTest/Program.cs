using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Adxl345;

namespace WasherTest
{
    class Program
    {
        static DateTime lastTrigger = DateTime.MinValue;
        static GpioController gpio = new GpioController(PinNumberingScheme.Logical);
        private static bool AllowTriggerRelay = false;
        private static GpioRelay relay = new GpioRelay(23, gpio);
        private static float RollingMagnitudeAverage = 0;
        private static FixedSizedQueue<string> messageQueue = new FixedSizedQueue<string>(5);
        private static float MaxMagnitude = 0.1f;

        private static GpioButton button = new GpioButton(26, gpio);
        static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            SpiConnectionSettings settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = Adxl345.SpiClockFrequency,
                Mode = Adxl345.SpiMode
            };
            var device = SpiDevice.Create(settings);
            //device.TransferFullDuplex(); -< read write registers using this
            // Set gravity measurement range ±4G
            using Adxl345 sensor = new Adxl345(device, GravityRange.Range04);
            var windowSize = 16;
            var avgValues = new FixedSizedQueue<double>(windowSize);
            var logTime = DateTime.MinValue;
            StreamWriter sw = null;
            button.Pressed += (sender, eventArgs) =>
            {
                //AddMessageToQueue("Button pressed. Manually triggering relay");
                //relay.Activate();
                logTime = DateTime.Now + TimeSpan.FromMinutes(80);
                var fn = $"data_{DateTime.Now.ToFileTimeUtc()}.csv";
                sw?.Flush();
                sw?.Dispose();
                sw = new StreamWriter(File.OpenWrite(fn));
                sw.WriteLine("X,Y,Z");
                sw.Flush();
                AddMessageToQueue($"Button pressed. Logging to ({fn}) until [{logTime}]");
            };
            while (true)
            {
                // read data
                Vector3 data = sensor.Acceleration;

                if (DateTime.Now < logTime)
                {
                    sw?.WriteLine($"{data.X},{data.Y},{data.Z}");//write raw data to file
                }
                else if(sw != null)
                {
                    sw?.Flush();
                    sw?.Dispose();
                    sw = null;
                    AddMessageToQueue($"Done logging");
                }

                var magnitude = Math.Sqrt(Math.Pow(data.X, 2) + Math.Pow(data.Y, 2) + Math.Pow(data.Z, 2));
                magnitude = Math.Abs(magnitude - 1);
                //avgValues.Enqueue(magnitude);
                RollingMagnitudeAverage = (float) ApproxRollingAverage(RollingMagnitudeAverage, magnitude, windowSize);
                if (RollingMagnitudeAverage >= MaxMagnitude)
                {
                    AddMessageToQueue($"[{DateTime.Now}] Logic Triggered: Magnitude Average: {RollingMagnitudeAverage}");
                    if (lastTrigger <= DateTime.Now - TimeSpan.FromSeconds(30))
                    {
                        AddMessageToQueue($"[{DateTime.Now}] Relay Triggered");
                        lastTrigger = DateTime.Now;
                        if (AllowTriggerRelay)
                        {
                            relay.Activate();
                        }
                    }
                }
                Console.Clear();
                Console.WriteLine($"{data:F2} g | {Math.Round(magnitude, 2)} m| {Math.Round(RollingMagnitudeAverage, 2)} m");
                foreach (var msg in messageQueue)
                {
                    Console.WriteLine(msg);
                }
                Thread.Sleep(250);
            }
        }

 

        private static void AddMessageToQueue(string message, int msToRemove = 10000)
        {
            messageQueue.Enqueue(message);
            Task.Delay(msToRemove).ContinueWith(task => messageQueue.TryDequeue(out _));
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            //File.WriteAllText("data.csv", sb.ToString());
            button?.Dispose();
            relay?.Dispose();
            gpio?.Dispose();
        }

        public static double ApproxRollingAverage(double avg, double new_sample, int N)
        {

            avg -= avg / N;
            avg += new_sample / N;
            return avg;
        }
    }
}
