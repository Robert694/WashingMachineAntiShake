using System.Threading.Tasks;
using Washer.Gpio;

namespace Washer.ShakeHandling
{
    public class RelayShakeHandler : IShakeHandler
    {
        private readonly GpioRelay _relay;

        public RelayShakeHandler(GpioRelay relay)
        {
            _relay = relay;
        }

        public async Task Trigger() => await _relay.Activate();
    }
}