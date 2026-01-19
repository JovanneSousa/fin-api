using AutoMapper;
using fin_api.DTOs;
using fin_api.Models;

namespace fin_api.Configuration.Mapper
{
    public class TransacaoProfile : Profile
    {
        public TransacaoProfile()
        {
            CreateMap<Transacao, TransacaoDTO>();
            CreateMap<TransacaoDTO, Transacao>();
        }
    }
}
