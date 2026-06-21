using AutoMapper;
using Fin.Infra.DTOs;
using Fin.Infra.Notificacoes;
using Fin.Infra.Repositories;
using Fin.Domain.Models;

namespace Fin.Application.Services
{
    public class UsuarioService : BaseService, IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapper _mapper;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            INotificador notificador,
            IMapper mapper
            )
            : base(notificador)
        {
            _usuarioRepository = usuarioRepository;
            _mapper = mapper;
        }

        public async Task<bool> CriarUsuarioAsync(Usuario usuario)
        {
            var result = await ExecuteAsync(
                async () => await _usuarioRepository.CreateUsuarioAsync(usuario)
                );

            if(!result)
            {
                _notificador.Handle(new Notificacao("Erro ao salvar usuario!"));
                return false;
            }
            return true;
        }

        public async Task<UsuarioDTO> BuscarUsuarioPorIdAsync(string id)
        {
            var user = await ExecuteAsync(
                async () => await _usuarioRepository.GetUsuarioByIdAsync(id)
                );
            if(user == null)
            {
                _notificador.Handle(new Notificacao("Usuario não encontrado!"));
                return null;
            }

            return _mapper.Map<UsuarioDTO>(user);
        }
    }
}
