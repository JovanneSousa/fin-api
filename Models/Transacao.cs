using fin_api.Enums;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace fin_api.Models
{
    public class Transacao : Entity
    {
        public string UserId { get; set; }
        public TransacaoType Type { get; set; }
        public string Titulo { get; set; }
        public decimal Valor { get; set; }
        public string CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRecurring { get; set; }
        public RecorrenciaType? RecorrenciaType { get; set; }
        public DateTime? RecorrenciaEndDate { get; set; }
        public int? Parcelas { get; set; }
        public int? ParcelaAtual { get; set; }
        public string? ParentTransactionId { get; set; }
        public Transacao ParentTransaction { get; set; }
        public bool ParcelaValida(int parcelas)
            => parcelas >= 2;
    }
}
