using System;
using System.Device.Gpio;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Washer.Gpio;
using Washer.Service;
using Washer.ShakeHandling;

namespace Washer
{
    public class Startup
    {
        public string[] Args { get; }

        public Startup(string[] args)
        {
            Args = args;
        }

        public static Task RunAsync(string[] args) => new Startup(args).RunAsync();

        public async Task RunAsync()
        {
            IServiceCollection services = new ServiceCollection(); // Create a new instance of a service collection
            await ConfigureServices(services); //Register
            await using var provider = services.BuildServiceProvider();
            await provider.GetRequiredService<StartupService>().StartAsync(); // Start the startup service
            await provider.GetRequiredService<WasherMonitor>().StartAsync(); // Start the washer service
            provider.GetRequiredService<ManualButtonHandler>();
            await Task.Delay(-1); // Keep the program alive
        }

        private async Task ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<StartupService>()
                .AddSingleton<WasherMonitor>()
                .AddSingleton<ManualButtonHandler>()
                .AddSingleton(x => new WasherMonitorSettings()
                {
                    TicksPerSecond = 4,
                    WindowSize = 16,
                    MaxThreshold = 0.25f
                })
                .AddSingleton(x => new GpioController(PinNumberingScheme.Logical)) //create gpio controller
                .AddSingleton(x => new GpioButton(26, x.GetRequiredService<GpioController>())) // create gpio button
                .AddSingleton(x => new GpioRelay(23, x.GetRequiredService<GpioController>())) // create gpio relay
                .AddSingleton<IShakeHandler>(x =>
                    new ThrottledShakeHandler(//throttled shake handler 
                        new LoggingShakeHandler( //log when shake
                            new RelayShakeHandler(//relay shake handler
                                x.GetRequiredService<GpioRelay>()),
                            x.GetRequiredService<ILogger<LoggingShakeHandler>>()),//logging
                        TimeSpan.FromMinutes(1)) //throttled min interval
                )
                .AddSingleton<IAccelerometer, DefaultAccelerometer>() // create accelerometer
                .AddLogging(x =>
                    x
                        .SetMinimumLevel(LogLevel.Debug)
                        .AddSimpleConsole(options =>
                        {
                            options.IncludeScopes = true;
                            options.SingleLine = true;
                            options.TimestampFormat = "MM/dd/yyyy hh:mm:ss tt|";
                        }));
        }
    }
}