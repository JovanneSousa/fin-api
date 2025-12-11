using fin_api.Extensions;
using fin_api.Notificacoes;
using fin_api.Repositories;
using fin_api.Services;

namespace fin_api.Configuration
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

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return builder;
        }
    }
}
