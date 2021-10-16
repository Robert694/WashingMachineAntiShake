using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Washer.Service
{
    public class StartupService
    {
        private readonly IServiceProvider _provider;

        private readonly ILogger _logger;
        static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        static readonly string Name = Assembly.GetExecutingAssembly().GetName().Name;

        public StartupService(
            IServiceProvider provider,
            ILogger<StartupService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            var title = $"{Name} - Version: {Version}";
            try
            {
                Console.Title = title;
            }
            catch
            {
            }
            _logger.LogInformation(title);
        }
    }
}