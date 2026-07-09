using Bus;
using Messages;
using Messages.Integration;
using FluentValidation.Results;
using Fin.Application.Notificacoes;
using Fin.Domain.Models;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Fin.Application.Interfaces.Services;

namespace Fin.Application.Services
{
    public class RegistroUsuarioIntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RegistroUsuarioIntegrationHandler> _logger;

        public RegistroUsuarioIntegrationHandler
            (
                IMessageBus bus,
                IServiceScopeFactory scopeFactory,
                ILogger<RegistroUsuarioIntegrationHandler> logger
            )
        {
            _bus = bus;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando Consumer RegistroUsuario");

            await _bus.RespondAsync<UsuarioRegistradoIntegrationEvent, ResponseMessage>(RegistrarUsuario, stoppingToken);

            await Task.CompletedTask;

            _logger.LogInformation("Consumer registrado no RabbitMQ");
        }

        private async Task<ResponseMessage> RegistrarUsuario(UsuarioRegistradoIntegrationEvent usuarioMessage)
        {
            _logger.LogInformation(
                $"Mensagem recebida para registro de usuário: {usuarioMessage.Nome}");

            var scope = _scopeFactory.CreateScope();

            var (_mapper, _usuarioService, _notificador) 
                = ConfiguraDependencias(ConfiguraScopo(scope));

            var usuario = _mapper.Map<Usuario>(usuarioMessage);

            if (!usuario.EhValido())
            {
                _logger.LogWarning("Usuário inválido");
                return new ResponseMessage(usuario.ValidationResult);
            }

            await _usuarioService.CriarUsuarioAsync(usuario);

            var notificacoes = _notificador.ObterNotificacoes();

            if (_notificador.TemNotificacao())
            {
                _logger.LogError($"Falha ao registrar usuario, erros: {notificacoes.ToString()}");

                var validationResult = new ValidationResult(
                    notificacoes.Select(n => 
                        new ValidationFailure("Usuario", n.Mensagem))
                    ); 
                return new ResponseMessage(validationResult);
            }

            _logger.LogInformation($"Usuário criado com sucesso {usuario.Id}");
            return new ResponseMessage(new ValidationResult());
        }

        private IServiceProvider ConfiguraScopo(IServiceScope scope)
            => scope.ServiceProvider;

        private (IMapper _mapper, IUsuarioService _usuarioSerivce, INotificador _notificador) 
            ConfiguraDependencias(IServiceProvider provider)
        {
            var _usuarioService = provider.GetRequiredService<IUsuarioService>();
            var _notificador = provider.GetRequiredService<INotificador>();
            var _mapper = provider.GetRequiredService<IMapper>();

            return (_mapper, _usuarioService, _notificador);
        }
    }
}
