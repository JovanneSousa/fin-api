using Serilog;

namespace Fin.Api.Configuration
{
    public static class SerilogConfig
    {
        public static IServiceCollection AddSerilogConfig(this IServiceCollection services, IHostBuilder host)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .CreateLogger();

            host.UseSerilog();

            return services;
        }
    }
}
