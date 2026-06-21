using Serilog;

namespace Fin.Api.Configuration
{
    public static class SerilogConfig
    {
        public static WebApplicationBuilder AddSerilogConfig(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

            return builder;
        }
    }
}
