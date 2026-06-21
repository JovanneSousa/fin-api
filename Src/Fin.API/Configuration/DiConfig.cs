using Fin.Infra.Notificacoes;
using Fin.Infra.Repositories;
using Fin.Application.Services;
using Fin.Api.Extensions;

namespace Fin.Api.Configuration
{
    public static class DiConfig
    {
        public static WebApplicationBuilder AddDiConfig(this WebApplicationBuilder builder)
        {

            builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();
            builder.Services.AddScoped<ITransacaoService, TransactionService>();
            builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            builder.Services.AddScoped<ICategoriaService, CategoriaService>();
            builder.Services.AddScoped<INotificador, Notificador>();
            builder.Services.AddScoped<IUser, AspNetUser>();
            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();

            builder.Services.AddHostedService<RegistroUsuarioIntegrationHandler>();

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return builder;
        }
    }
}
