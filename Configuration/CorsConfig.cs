namespace fin_api.Configuration
{
    public static class CorsConfig
    {
        public static WebApplicationBuilder AddCorsConfig(this WebApplicationBuilder builder)
        {
            var allowedOrigin = builder.Configuration
                .GetSection("MEU_APP")
                .GetChildren()
                .Select(x => x.Value)
                .ToArray();

            if (allowedOrigin == null || allowedOrigin.Length == 0)
                throw new InvalidOperationException("Nenhuma origem configurada em 'MEU_APP'");

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
