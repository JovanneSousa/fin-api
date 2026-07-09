using AutoMapper;
using Fin.Application.DTOs;
using Fin.Application.Notificacoes;
using Fin.Domain.Models;
using Fin.Application.Interfaces.Repositories;
using Fin.Application.Interfaces.Services;

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

            if (!result)
                return RetornaErroProcessamento<bool>("Erro ao salvar usuario!");

            return true;
        }

        public async Task<UsuarioDTO> BuscarUsuarioPorIdAsync(string id)
        {
            var user = await ExecuteAsync(
                async () => await _usuarioRepository.GetUsuarioByIdAsync(id)
                );
            if (user == null)
                return RetornaErroProcessamento<UsuarioDTO>("Usuario não encontrado!");

            return _mapper.Map<UsuarioDTO>(user);
        }
    }
}
