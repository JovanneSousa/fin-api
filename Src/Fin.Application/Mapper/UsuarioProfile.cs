using AutoMapper;
using Messages.Integration;
using Fin.Domain.Models;
using Fin.Application.DTOs;

namespace Fin.Application.Mapper
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<Usuario, UsuarioRegistradoIntegrationEvent>();
            CreateMap<UsuarioRegistradoIntegrationEvent, Usuario>();

            CreateMap<Usuario, UsuarioDTO>();
            CreateMap<UsuarioDTO, Usuario>();
        }
    }
}
