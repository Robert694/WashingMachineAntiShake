using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Washer.ShakeHandling
{
    public class LoggingShakeHandler : IShakeHandler
    {
        private readonly IShakeHandler _inner;
        private readonly ILogger<LoggingShakeHandler> _logger;

        public LoggingShakeHandler(IShakeHandler inner, ILogger<LoggingShakeHandler> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public Task Trigger()
        {
            _logger.LogInformation("Shake handler triggered");
            return _inner.Trigger();
        }
    }
}