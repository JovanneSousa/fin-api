using Fin.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Fin.Api.Configuration
{
    public static class DbContextConfig
    {
        public static WebApplicationBuilder AddDbContextConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<ApiDbContext>(o =>
            {
                var connectionString =
                    builder.Configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("Connection string não configurada.");

                o.UseNpgsql(connectionString);
            });

            return builder;
        }
    }
}
