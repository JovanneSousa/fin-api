using AutoMapper;
using Fin.Domain.Models;
using Fin.Infra.DTOs;

namespace Fin.Application.Mapper
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
