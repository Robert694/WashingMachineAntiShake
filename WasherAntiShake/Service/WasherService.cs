using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;
using Washer.Gpio;
using Washer.ShakeHandling;

namespace Washer.Service
{
    public class WasherMonitor
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger _logger;
        private readonly IAccelerometer _accelerometer;
        private readonly IShakeHandler _shakeHandler;
        private readonly WasherMonitorSettings _settings;
        private readonly System.Timers.Timer _timer;
        private readonly IValueCalc Calc;
        public WasherMonitor(
            IServiceProvider provider,
            ILogger<WasherMonitor> logger, 
            IAccelerometer accelerometer,
            IShakeHandler shakeHandler,
            WasherMonitorSettings settings)
        {
            _provider = provider;
            _logger = logger;
            _accelerometer = accelerometer;
            _shakeHandler = shakeHandler;
            _settings = settings;
            _timer = new System.Timers.Timer(1000/settings.TicksPerSecond);
            _timer.Elapsed += TimerOnElapsed;
            Calc = new DeltaValueCalc2(settings.WindowSize);
        }

        public async Task StartAsync()
        {
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var value = Calc.GetValue(_accelerometer.Acceleration);
            if (value >= _settings.MaxThreshold)
            {
                _shakeHandler.Trigger();
                _logger.LogInformation($"{value}");
            }

        }
    }

    public interface IValueCalc
    {
        double GetValue(Vector3 acceleration);
    }

    public class ValueCalc1 : IValueCalc
    {
        private readonly int _windowSize;
        private double _rollingMagnitude;

        public ValueCalc1(int windowSize)
        {
            _windowSize = windowSize;
        }

        public double GetValue(Vector3 acceleration)
        {
            var magnitude = acceleration.Magnitude();
            magnitude = Math.Abs(magnitude - 1);
            _rollingMagnitude = ApproxRollingAverage(_rollingMagnitude, magnitude, _windowSize);
            return _rollingMagnitude;
        }

        public static double ApproxRollingAverage(double avg, double newSample, int n)
        {
            avg -= avg / n;
            avg += newSample / n;
            return avg;
        }
    }

    public class DeltaValueCalc : IValueCalc
    {
        private readonly int _windowSize;
        private readonly FixedSizedQueue<double> _magnitudes;

        public DeltaValueCalc(int windowSize)
        {
            _windowSize = windowSize;
            _magnitudes = new FixedSizedQueue<double>(windowSize);
        }

        public double GetValue(Vector3 acceleration)
        {
            var magnitude = Math.Sqrt(Math.Pow(acceleration.X, 2) + Math.Pow(acceleration.Y, 2) + Math.Pow(acceleration.Z, 2));
            _magnitudes.Enqueue(magnitude);
            var minMag = _magnitudes.Min();
            var maxMag = _magnitudes.Max();
            var delta = Math.Abs(maxMag - minMag);
            return delta;
        }
    }

    public class DeltaValueCalc2 : IValueCalc
    {
        private readonly int _windowSize;
        private readonly FixedSizedQueue<double> _magnitudes;
        private readonly FixedSizedQueue<double> minMagnitudes;
        private readonly FixedSizedQueue<double> maxMagnitudes;
        private readonly FixedSizedQueue<double> deltas;

        public DeltaValueCalc2(int windowSize)
        {
            _windowSize = windowSize;
            _magnitudes = new FixedSizedQueue<double>(windowSize);
            minMagnitudes = new FixedSizedQueue<double>(windowSize * 4);
            maxMagnitudes = new FixedSizedQueue<double>(windowSize * 4);
            deltas = new FixedSizedQueue<double>(windowSize);
        }

        public double GetValue(Vector3 acceleration)
        {
            var magnitude = Math.Sqrt(Math.Pow(acceleration.X, 2) + Math.Pow(acceleration.Y, 2) + Math.Pow(acceleration.Z, 2));
            _magnitudes.Enqueue(magnitude);
            var minMag = _magnitudes.Min();
            var maxMag = _magnitudes.Max();
            minMagnitudes.Enqueue(minMag);
            maxMagnitudes.Enqueue(maxMag);
            var minMagsAvg = minMagnitudes.Average();
            var maxMagsAvg = maxMagnitudes.Average();
            var delta = Math.Abs(maxMagsAvg - minMagsAvg);
            //deltas.Enqueue(delta);
            //var avgdelta = deltas.Average();
            return delta;
        }
    }
}