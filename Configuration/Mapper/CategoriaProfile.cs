using AutoMapper;
using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Configuration.Mapper
{
    public class CategoriaProfile : Profile
    {
        public CategoriaProfile()
        {
            CreateMap<Categoria, CategoriaDTO>();
            CreateMap<CategoriaDTO, Categoria>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CategoriaUpdateDTO, Categoria>()
                .ForMember(d => d.IconePadrao, o => o.Ignore())
                .ForMember(d => d.CorPadrao, o => o.Ignore())
                .ForMember(d => d.IconeCategoriaUsuario, o => o.Ignore())
                .ForMember(d => d.CorCategoriaUsuarios, o => o.Ignore())
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.IsDefault, o => o.Ignore())
                .ForAllMembers(o =>
                    o.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
