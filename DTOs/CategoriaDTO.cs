using fin_api.Enums;
using fin_api.Models;
using System.Text.Json.Serialization;

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
        public string Id { get; set; }

        /// <summary>
        /// Nome da categoria (ex: Alimentação, Transporte, Lazer).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tipo de transação associada (Entrada ou Saída).
        /// </summary>
        public TransacaoType Type { get; set; }

        /// <summary>
        /// ID do usuário dono da categoria.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// ID do ícone associado à categoria.
        /// </summary>
        public string IconId { get; set; }

        /// <summary>
        /// Objeto de detalhes do ícone.
        /// </summary>
        public IconDTO Icone { get; set; }

        /// <summary>
        /// ID da cor associada à categoria.
        /// </summary>
        public string CorId { get; set; }

        /// <summary>
        /// Objeto de detalhes da cor.
        /// </summary>
        public CorDTO Cor { get; set; }
    }
}
