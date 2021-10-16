using System;
using Microsoft.Extensions.Logging;
using Washer.Gpio;

namespace Washer
{
    public class ManualButtonHandler
    {
        private readonly GpioButton _button;
        private readonly GpioRelay _relay;
        private readonly ILogger<ManualButtonHandler> _logger;

        public ManualButtonHandler(
            GpioButton button,
            GpioRelay relay,
            ILogger<ManualButtonHandler> logger)
        {
            _button = button;
            _relay = relay;
            _logger = logger;
            _button.Pressed += ButtonOnPressed;
        }

        private void ButtonOnPressed(object sender, EventArgs e)
        {
            _logger.LogInformation("Pressed manual button");
            _relay.Activate();
        }
    }
}