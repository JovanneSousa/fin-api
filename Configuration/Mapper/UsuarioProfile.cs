using AutoMapper;
using Messages.Integration;
using fin_api.Models;

namespace fin_api.Configuration.Mapper
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<Usuario, UsuarioRegistradoIntegrationEvent>();
            CreateMap<UsuarioRegistradoIntegrationEvent, Usuario>();
        }
    }
}
