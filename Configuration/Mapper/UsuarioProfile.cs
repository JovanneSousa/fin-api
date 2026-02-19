using AutoMapper;
using Messages.Integration;
using fin_api.Models;
using fin_api.DTOs;

namespace fin_api.Configuration.Mapper
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
