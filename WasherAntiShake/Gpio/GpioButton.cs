using System;
using System.Device.Gpio;

namespace Washer.Gpio
{
    public class GpioButton : IDisposable
    {
        public int PinNumber { get; }
        private readonly GpioController _controller;
        private System.Timers.Timer timer = new System.Timers.Timer(500);

        public GpioButton(int pinNumber, GpioController controller = null, PinMode pinMode = PinMode.Input)
        {
            PinNumber = pinNumber;
            _controller = controller ?? new GpioController();
            timer.Elapsed += (sender, args) =>
            {
                if (StateChanges > 0)
                {
                    if (--StateChanges == 0)
                    {
                        ButtonPressed = false;
                        Released?.Invoke(this, EventArgs.Empty);
                    }
                }
            };
            timer.Start();
            _controller.OpenPin(PinNumber, PinMode.Input);
            _controller.RegisterCallbackForPinValueChangedEvent(PinNumber, PinEventTypes.Rising, Callback);
            _controller.RegisterCallbackForPinValueChangedEvent(PinNumber, PinEventTypes.Falling, Callback);
        }

        public event EventHandler Pressed;
        public event EventHandler Released;
        private int StateChanges = 0;
        private bool ButtonPressed;

        private void Callback(object sender, PinValueChangedEventArgs pinvaluechangedeventargs)
        {
            //Console.WriteLine(pinvaluechangedeventargs.ChangeType.ToString());
            if (StateChanges < 4) StateChanges++;

            if (StateChanges > 1 && !ButtonPressed)
            {
                ButtonPressed = true;
                Pressed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            timer?.Dispose();
            _controller?.UnregisterCallbackForPinValueChangedEvent(PinNumber, Callback);
            _controller?.ClosePin(PinNumber);
        }
    }
}