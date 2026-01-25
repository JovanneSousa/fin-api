using fin_api.Enums;
using fin_api.Models;

namespace fin_api.DTOs
{
    public class TransacaoDTO 
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public TransacaoType Type { get; set; }
        public string Titulo { get; set; }
        public decimal Valor { get; set; }
        public string CategoriaId { get; set; }
        public CategoriaDTO Categoria { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public bool IsRecurring { get; set; }
        public RecorrenciaType? RecorrenciaType { get; set; }
        public DateTime? RecorrenciaEndDate { get; set; }
        public int? Parcelas { get; set; }
        public int? ParcelaAtual { get; set; }
        public bool ParcelaValida(int parcelas)
            => parcelas >= 2;
    }
}
