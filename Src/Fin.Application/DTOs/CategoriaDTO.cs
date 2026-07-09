using Fin.Domain.Enums;
using Fin.Domain.Models;
using System.Text.Json.Serialization;

namespace Fin.Application.DTOs
{
    /// <summary>
    /// Objeto que representa uma categoria de transação financeira.
    /// </summary>
    public record CategoriaDTO
    {
        /// <summary>
        /// Identificador único da categoria.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

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

        /// <summary>
        /// Categoria é padrão
        /// </summary>
        public bool IsDefault { get; set; }

        public static IEnumerable<CategoriaDTO> ToDtoList(IEnumerable<Categoria> categorys)
            => categorys.Select(c => CategoriaDTO.ToDto(c));

        public static CategoriaDTO ToDto(Categoria categoria)
        {
            if (categoria is null)
                return null!;

            return new CategoriaDTO
            {
                Id = categoria.Id,
                Name = categoria.Name,
                Type = categoria.Type,
                UserId = categoria.UserId,
                IconId = categoria.IconId,
                CorId = categoria.CorId,

                Icone = categoria.Icone is null
                    ? null!
                    : new IconDTO
                    {
                        Id = categoria.Icone.Id,
                        Name = categoria.Icone.Name,
                        Url = categoria.Icone.Url
                    },

                Cor = categoria.Cor is null
                    ? null!
                    : new CorDTO
                    {
                        Id = categoria.Cor.Id,
                        Url = categoria.Cor.Url,
                    }
            };
        }

        public static IEnumerable<Categoria> ToDomainList(IEnumerable<CategoriaDTO> categories)
            => categories.Select(c => c.ToDomain());

        public Categoria ToDomain()
        {

            return new Categoria
            {
                Id = Id,
                Name = Name,
                Type = Type,
                UserId = UserId,
                IconId = IconId,
                CorId = CorId
            };
        }
    }
}
