using fin_api.Models;
using fin_api.Notificacoes;
using fin_api.Repositories;

namespace fin_api.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly INotificador _notificador;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            INotificador notificador
            )
        {
            _usuarioRepository = usuarioRepository;
            _notificador = notificador;
        }

        public async Task<bool> CriarUsuarioAsync(Usuario usuario)
        {
            var result = await _usuarioRepository.CreateUsuarioAsync(usuario);
            if(!result)
            {
                _notificador.Handle(new Notificacao("Erro ao salvar usuario!"));
                return false;
            }
            return true;
        }
    }
}
