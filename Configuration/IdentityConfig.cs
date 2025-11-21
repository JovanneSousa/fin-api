using fin_api.Data;
using fin_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace fin_api.Configuration
{
    public static class IdentityConfig
    {
        public static WebApplicationBuilder AddIdentityConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApiDbContext>();

            // Pegando o token e gerando chave encodada
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));   

            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            if (string.IsNullOrEmpty(jwtSettings?.Segredo))
                throw new InvalidOperationException("Segredo JWT não configurado.");

            var key = Encoding.ASCII.GetBytes(jwtSettings.Segredo);

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = true;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audiencia,
                    ValidIssuer = jwtSettings.Emissor
                };
            });

            return builder;
        }
    }
}
