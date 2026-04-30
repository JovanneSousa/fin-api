using fin_api.Enums;

namespace fin_api.DTOs
{
    /// <summary>
    /// Objeto que representa uma categoria de transação financeira.
    /// </summary>
    public class CategoriaDTO
    {
        /// <summary>
        /// Identificador único da categoria.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Nome da categoria (ex: Alimentação, Transporte, Lazer).
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Tipo de transação associada (Entrada ou Saída).
        /// </summary>
        public required TransacaoType Type { get; set; }

        /// <summary>
        /// ID do usuário dono da categoria.
        /// </summary>
        public required string UserId { get; set; }

        /// <summary>
        /// ID do ícone associado à categoria.
        /// </summary>
        public required string IconId { get; set; }

        /// <summary>
        /// Objeto de detalhes do ícone.
        /// </summary>
        public required IconDTO Icone { get; set; }

        /// <summary>
        /// ID da cor associada à categoria.
        /// </summary>
        public required string CorId { get; set; }

        /// <summary>
        /// Objeto de detalhes da cor.
        /// </summary>
        public required CorDTO Cor { get; set; }
    }
}
