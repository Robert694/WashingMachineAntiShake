using System;
using System.Device.Gpio;
using System.Threading.Tasks;

namespace WasherTest
{
    public class GpioRelay : IDisposable
    {
        public int PinNumber { get; }
        private GpioController Controller { get; }
        public DateTime? LastClose;
        public DateTime? LastOpen;

        public GpioRelay(int pinNumber, GpioController controller = null)
        {
            PinNumber = pinNumber;
            Controller = controller ?? new GpioController();
            Controller.OpenPin(PinNumber, PinMode.Output);
        }

        public void DisableRelay()
        {
            Controller.Write(PinNumber, PinValue.Low);
        }

        /// <summary>
        /// When relay is being triggered
        /// </summary>
        public event EventHandler OnClose;

        /// <summary>
        /// When relay is no longer being triggered
        /// </summary>
        public event EventHandler OnOpen;

        /// <summary>
        /// Triggers relay for set amount of time in milliseconds
        /// </summary>
        /// <param name="closeTimeMs"></param>
        /// <returns></returns>
        public async Task Activate(int closeTimeMs = 1000)
        {
            Controller.Write(PinNumber, PinValue.High);
            Log(true);
            await Task.Delay(closeTimeMs);
            Controller.Write(PinNumber, PinValue.Low);
            Log(false);
        }

        /// <summary>
        /// Toggles the relay into the opposite state
        /// </summary>
        public void Toggle()
        {
            Controller.Write(PinNumber, Controller.Read(PinNumber) == PinValue.Low ? PinValue.High : PinValue.Low);
        }

        private void Log(bool close)
        {
            if (close)
            {
                OnClose?.Invoke(this, EventArgs.Empty);
                LastClose = DateTime.Now;
            }
            else
            {
                OnOpen?.Invoke(this, EventArgs.Empty);
                LastOpen = DateTime.Now;
            }
        }

        public void Dispose()
        {
            Controller?.ClosePin(PinNumber);
        }
    }
}