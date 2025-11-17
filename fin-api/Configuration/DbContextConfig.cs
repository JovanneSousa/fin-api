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
                var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
                o.UseNpgsql(connectionString);
            });

            return builder;
        }
    }
}
