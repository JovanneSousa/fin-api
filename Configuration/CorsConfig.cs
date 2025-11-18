namespace fin_api.Configuration
{
    public static class CorsConfig
    {
        public static WebApplicationBuilder AddCorsConfig(this WebApplicationBuilder builder)
        {
            var allowedOrigin = builder.Configuration["MEU_APP"];

            if (string.IsNullOrWhiteSpace(allowedOrigin))
                throw new Exception("A variável de ambiente 'MEU_APP' não foi definida.");

            builder.Services.AddCors(o =>
            {
                o.AddPolicy("Production", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigin)
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .AllowAnyHeader();
                });
            });

            return builder;
        }
    }
}
