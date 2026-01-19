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
            CreateMap<CategoriaDTO, Categoria>();
        }
    }
}
