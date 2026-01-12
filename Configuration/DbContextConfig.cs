using fin_api.Data;
using Microsoft.EntityFrameworkCore;

namespace fin_api.Configuration
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
