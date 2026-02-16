using fin_api.Models;
using fin_api.Notificacoes;

namespace fin_api.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioService _usuarioService;
        private readonly INotificador _notificador;

        public UsuarioService(
            IUsuarioService usuarioService,
            INotificador notificador
            )
        {
            _usuarioService = usuarioService;
            _notificador = notificador;
        }

        public async Task<bool> CriarUsuarioAsync(Usuario usuario)
        {
            var result = await _usuarioService.CriarUsuarioAsync(usuario);
            if(!result)
            {
                _notificador.Handle(new Notificacao("Erro ao salvar usuario!"));
                return false;
            }
            return true;
        }
    }
}
