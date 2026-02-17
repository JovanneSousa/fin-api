
using Bus;
using fin_api.Models;
using fin_api.Notificacoes;
using Messages;
using Messages.Integration;
using FluentValidation.Results;
using AutoMapper;

namespace fin_api.Services
{
    public class RegistroUsuarioIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceScopeFactory _scopeFactory;

        public RegistroUsuarioIntegrationHandler
            (
                IMessageBus bus,
                IServiceScopeFactory scopeFactory
            )
        {
            _bus = bus;
            _scopeFactory = scopeFactory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var disposable = 
                await _bus.RespondAsync<UsuarioRegistradoIntegrationEvent, ResponseMessage>(RegistrarUsuario, stoppingToken);

            await Task.CompletedTask;
        }

        private async Task<ResponseMessage> RegistrarUsuario(UsuarioRegistradoIntegrationEvent usuarioMessage)
        {
            using var scope = _scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;

            var _usuarioService = provider.GetRequiredService<IUsuarioService>();
            var _notificador = provider.GetRequiredService<INotificador>();
            var _mapper = provider.GetRequiredService<IMapper>();

            var usuario = _mapper.Map<Usuario>(usuarioMessage);

            if (!usuario.EhValido())
                return new ResponseMessage(usuario.ValidationResult);

            await _usuarioService.CriarUsuarioAsync(usuario);

            var notificacoes = _notificador.ObterNotificacoes();

            if (_notificador.TemNotificacao())
            {
                var validationResult = new ValidationResult(
                    notificacoes.Select(n => 
                        new ValidationFailure("Usuario", n.Mensagem))
                    ); 
                return new ResponseMessage(validationResult);
            }

            return new ResponseMessage(new ValidationResult());
        }
    }
}
