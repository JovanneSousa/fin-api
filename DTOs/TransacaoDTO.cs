using fin_api.Enums;
using fin_api.Models;

namespace fin_api.DTOs
{
    /// <summary>
    /// Representa uma transação financeira no sistema.
    /// </summary>
    public class TransacaoDTO 
    {
        /// <summary>
        /// Identificador único da transação.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// ID do usuário ao qual a transação pertence.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// Tipo da transação (Receita ou Despesa).
        /// </summary>
        public required TransacaoType Type { get; set; }

        /// <summary>
        /// Descrição ou título da transação.
        /// </summary>
        public required string Titulo { get; set; }

        /// <summary>
        /// Valor monetário da transação.
        /// </summary>
        public required decimal Valor { get; set; }

        /// <summary>
        /// ID da categoria associada a esta transação.
        /// </summary>
        public required string CategoriaId { get; set; }

        /// <summary>
        /// Detalhes da categoria.
        /// </summary>
        public required CategoriaDTO Categoria { get; set; }

        /// <summary>
        /// Data em que a transação ocorreu.
        /// </summary>
        public required DateTime DataMovimentacao { get; set; }

        /// <summary>
        /// Indica se a transação é recorrente.
        /// </summary>
        public required bool IsRecurring { get; set; }

        /// <summary>
        /// Tipo de recorrência (Mensal, Semanal, etc.).
        /// </summary>
        public RecorrenciaType? RecorrenciaType { get; set; }

        /// <summary>
        /// Data limite para o fim da recorrência.
        /// </summary>
        public DateTime? RecorrenciaEndDate { get; set; }

        /// <summary>
        /// Número total de parcelas (se aplicável).
        /// </summary>
        public int? Parcelas { get; set; }

        /// <summary>
        /// Número da parcela atual.
        /// </summary>
        public int? ParcelaAtual { get; set; }

        /// <summary>
        /// Verifica se o número de parcelas é válido para parcelamento.
        /// </summary>
        public bool ParcelaValida(int parcelas)
            => parcelas >= 2;
    }
}
