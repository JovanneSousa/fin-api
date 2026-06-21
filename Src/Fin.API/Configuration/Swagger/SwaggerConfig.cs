
using Microsoft.OpenApi;

namespace Fin.Api.Configuration.Swagger
{
    public static class SwaggerConfig
    {

        public static WebApplicationBuilder AddSwaggerConfig(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "FinControl API",
                    Version = "v1",
                    Description = "API de controle financeiro pessoal. Permite gerenciar categorias, transações e perfis de usuário.",
                    Contact = new OpenApiContact
                    {
                        Name = "Jovane Sousa",
                        Email = "jovanemaciel943@gmail.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<AuthResponsesOperationFilter>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Insira o token JWT desta maneira: Bearer {seu token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(
                    document => 
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    }
                   );
            });

            builder.Services.AddEndpointsApiExplorer();

            return builder;
        }
    }
}
