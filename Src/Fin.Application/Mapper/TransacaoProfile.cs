using AutoMapper;
using Fin.Domain.Models;
using Fin.Application.DTOs;

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
