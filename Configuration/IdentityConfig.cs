using fin_api.Data;
using fin_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtExtensions;
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
            Console.WriteLine(jwtSettings.AutenticacaoJwksUrl);
            if (string.IsNullOrEmpty(jwtSettings?.AutenticacaoJwksUrl))
                throw new InvalidOperationException("Url JWT não configurado.");

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                o.SaveToken = true;
                o.SetJwksOptions(new JwkOptions(jwtSettings.AutenticacaoJwksUrl + "/jwks", jwtSettings.AutenticacaoJwksUrl));
            });


            return builder;
        }
    }
}
