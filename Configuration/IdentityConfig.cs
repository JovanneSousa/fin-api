using fin_api.Data;
using fin_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using NetDevPack.Security.JwtExtensions;

namespace fin_api.Configuration
{
    public static class IdentityConfig
    {
        public static WebApplicationBuilder AddIdentityConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApiDbContext>();  

            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (string.IsNullOrEmpty(jwtSettings?.AutenticacaoJwksUrl))
                throw new InvalidOperationException("Url JWT não configurado.");
            var issuer = jwtSettings.Issuer ?? jwtSettings.AutenticacaoJwksUrl;

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                o.SaveToken = true;
                o.SetJwksOptions(new JwkOptions($"{jwtSettings.AutenticacaoJwksUrl}/jwks", issuer));
            });


            return builder;
        }
    }
}
