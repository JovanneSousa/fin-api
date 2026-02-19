using AutoMapper;
using fin_api.DTOs;
using fin_api.Models;
using fin_api.Notificacoes;
using fin_api.Repositories;

namespace fin_api.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly INotificador _notificador;
        private readonly IMapper _mapper;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            INotificador notificador,
            IMapper mapper
            )
        {
            _usuarioRepository = usuarioRepository;
            _notificador = notificador;
            _mapper = mapper;
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

        public async Task<UsuarioDTO> BuscarUsuarioPorIdAsync(string id)
        {
            var user = await _usuarioRepository.GetUsuarioByIdAsync(id);
            if(user == null)
            {
                _notificador.Handle(new Notificacao("Usuario não encontrado!"));
                return null;
            }

            return _mapper.Map<UsuarioDTO>(user);
        }
    }
}
