
using Fin.Application.DTOs;
using Fin.Application.Interfaces.Services;
using Fin.Application.Notificacoes;
using Jovanne.Jwks.Client.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fin.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController: MainController
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriesController(
            ICategoriaService service,
            INotificador notificador,
            IUser appUser) : base (notificador, appUser)
        {
            _categoriaService = service;
        }

        /// <summary>
        /// Obtém uma categoria pelo seu identificador único.
        /// </summary>
        /// <param name="id">O ID da categoria.</param>
        /// <returns>A categoria encontrada.</returns>
        /// <response code="200">Retorna a categoria solicitada.</response>
        /// <response code="404">Se a categoria não for encontrada.</response>
        [HttpGet("{id}")]
        [ClaimsAuthorize("permission", "FIN:CTG_LER")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<CategoriaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoriaDTO>> ObterCategoriaPorId(string id)
            => CustomResponse(await _categoriaService.ObterCategoriaId(id, UsuarioId));

        /// <summary>
        /// Lista todas as categorias cadastradas para o usuário autenticado.
        /// </summary>
        /// <returns>Uma lista de categorias.</returns>
        /// <response code="200">Retorna a lista de categorias.</response>
        [HttpGet]
        [ClaimsAuthorize("permission", "FIN:CTG_LER")]
        [ProducesResponseType(typeof(IEnumerable<CategoriaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> ListarCategorias() 
            => CustomResponse(await _categoriaService.ListCategoriasAsync(UsuarioId));

        /// <summary>
        /// Obtém a lista de ícones disponíveis para as categorias.
        /// </summary>
        /// <returns>Uma lista de ícones.</returns>
        /// <response code="200">Retorna a lista de ícones.</response>
        [HttpGet("icones")]
        [ClaimsAuthorize("permission", "FIN:CTG_LER")]
        [ProducesResponseType(typeof(IEnumerable<IconDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<IconDTO>>> ListarIcones()
            => CustomResponse(await _categoriaService.ListarIconesAsync());

        /// <summary>
        /// Obtém a lista de cores disponíveis para as categorias.
        /// </summary>
        /// <returns>Uma lista de cores.</returns>
        /// <response code="200">Retorna a lista de cores.</response>
        [HttpGet("cores")]
        [ClaimsAuthorize("permission", "FIN:CTG_LER")]
        [ProducesResponseType(typeof(IEnumerable<CorDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<CorDTO>>> ListarCores()
            => CustomResponse(await _categoriaService.ListarCoresAsync());

        /// <summary>
        /// Cadastra uma nova categoria para o usuário.
        /// </summary>
        /// <param name="categoria">Dados da nova categoria.</param>
        /// <returns>Os dados da categoria cadastrada.</returns>
        /// <response code="200">Categoria cadastrada com sucesso.</response>
        /// <response code="400">Se houver erros de validação.</response>
        [HttpPost]
        [ClaimsAuthorize("permission", "FIN:CTG_CRIAR")]
        [ProducesResponseType(typeof(CategoriaDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoriaDTO>> Cadastrar([FromBody] CategoriaDTO categoria)
            => CustomResponse(await _categoriaService.CreateCategoriaAsync(UsuarioId, categoria));

        /// <summary>
        /// Atualiza os dados de uma categoria existente.
        /// </summary>
        /// <param name="categoria">Novos dados da categoria.</param>
        /// <param name="id">O ID da categoria a ser atualizada.</param>
        /// <returns>A categoria atualizada.</returns>
        /// <response code="200">Categoria atualizada com sucesso.</response>
        /// <response code="400">Se houver erros de validação.</response>
        [HttpPut("atualizar/{id}")]
        [ClaimsAuthorize("permission", "FIN:CTG_CRIAR")]
        [ProducesResponseType(typeof(CategoriaDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoriaDTO>> AtualizarCategoria([FromBody] CategoriaUpdateDTO categoria, string id)
            => CustomResponse(await _categoriaService.AtualizarCategoria(categoria, UsuarioId, id));

        /// <summary>
        /// Exclui uma categoria do sistema.
        /// </summary>
        /// <param name="id">O ID da categoria a ser excluída.</param>
        /// <returns>Status de sucesso.</returns>
        /// <response code="200">Categoria excluída com sucesso.</response>
        /// <response code="404">Se a categoria não for encontrada ou não puder ser excluída.</response>
        [HttpDelete("{id}")]
        [ClaimsAuthorize("permission", "FIN:CTG_EXCLUIR")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<bool>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> Deletar(string id)
            => CustomResponse(!await _categoriaService.DeleteCategoriaAsync(UsuarioId, id));
    }
}
