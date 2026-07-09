using Fin.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Fin.Api.Configuration
{
    public static class DbContextConfig
    {
        public static IServiceCollection AddDbContextConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApiDbContext>(o =>
            {
                var connectionString =
                    configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Connection string não configurada.");

                o.UseNpgsql(connectionString);

                o.LogTo(Console.WriteLine, LogLevel.Information);

                o.EnableSensitiveDataLogging();
            });

            return services;
        }
    }
}
