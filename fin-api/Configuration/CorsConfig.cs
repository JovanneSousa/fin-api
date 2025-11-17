namespace fin_api.Configuration
{
    public static class CorsConfig
    {
        public static WebApplicationBuilder AddCorsConfig(this WebApplicationBuilder builder)
        {
            var webapp = builder.Configuration["MEU_APP"];
            builder.Services.AddCors(o =>
            {
                o.AddPolicy("Production", builder =>
                builder
                     .WithOrigins(webapp)
                     .WithMethods("POST, GET")
                     .AllowAnyHeader());
            });

            return builder;
        }
    }
}
