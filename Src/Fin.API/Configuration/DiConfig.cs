using Fin.Infra.Notificacoes;
using Fin.Infra.Repositories;
using Fin.Application.Services;
using Fin.Application.Interfaces.Repositories;
using Fin.Application.Interfaces.Services;
using Fin.Application.Notificacoes;
using Jovanne.Jwks.Client.Extensions;

namespace Fin.Api.Configuration
{
    public static class DiConfig
    {
        public static IServiceCollection AddDiConfig(this IServiceCollection services)
        {

            services.AddScoped<ITransacaoRepository, TransacaoRepository>();
            services.AddScoped<ITransacaoService, TransactionService>();
            services.AddScoped<ICategoriaRepository, CategoriaRepository>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IUser, AspNetUser>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IUsuarioService, UsuarioService>();

            services.AddHostedService<RegistroUsuarioIntegrationHandler>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }
    }
}
