namespace fin_api.Configuration
{
    public static class CorsConfig
    {
        public static WebApplicationBuilder AddCorsConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(o =>
            {
                o.AddPolicy("Development", builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
                o.AddPolicy("Production", builder =>
                builder
                     .WithOrigins("https://localhost:9000")
                     .WithMethods("POST")
                     .AllowAnyHeader());
            });

            return builder;
        }
    }
}
