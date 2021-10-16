using System;
using System.Threading.Tasks;

namespace Washer.ShakeHandling
{
    public class ThrottledShakeHandler : IShakeHandler
    {
        private readonly IShakeHandler _inner;
        private readonly TimeSpan _minInterval;
        private DateTime LastTrigger = DateTime.MinValue;

        public ThrottledShakeHandler(IShakeHandler inner, TimeSpan minInterval)
        {
            _inner = inner;
            _minInterval = minInterval;
        }

        public async Task Trigger()
        {
            if (DateTime.Now > (LastTrigger + _minInterval))
            {
                LastTrigger = DateTime.Now;
                await _inner.Trigger();
            }
        }
    }
}