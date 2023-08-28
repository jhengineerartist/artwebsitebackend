using Serilog;
using Serilog.Events;

namespace ArtWebsiteAPI
{
    public class InitializationUtils
    {
        public static void ConfigureSerilog(WebApplicationBuilder builder)
        {
            // Configure Serilog using the logging builder
            builder.Logging.Services.AddSerilog((context, configuration) =>
            {
                // Configure Serilog using the context and configuration parameters
                configuration
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(LogEventLevel.Information, "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.File("logs/log.txt", LogEventLevel.Debug, rollingInterval: RollingInterval.Day);
            });
        }
    }
}