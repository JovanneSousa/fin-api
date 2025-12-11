using fin_api.Extensions;
using fin_api.Models;
using fin_api.Notificacoes;
using fin_api.Repositories;
using fin_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fin_api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController: MainController
    {
        private readonly ICategoriaService _categoriaService;

        private readonly ITransacaoService _transacaoService;

        public CategoriesController(
            ICategoriaService service, 
            ITransacaoService transacaoService,
            INotificador notificador,
            IUser appUser) : base (notificador, appUser)
        {
            _categoriaService = service;
            _transacaoService = transacaoService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> ListarCategorias() =>
            CustomResponse(await _categoriaService.ListCategoriasAsync(UsuarioId));


        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] Categoria categoria)
            => CustomResponse(await _categoriaService.CreateCategoriaAsync(UsuarioId, categoria));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(string id)
        {

            var categoria = await _categoriaService.GetCategoriaAsync(id);
            if (categoria == null)
                return NotFound(new { message = "Categoria não encontrada." });

            if (!UsuarioAutenticado)
                return Unauthorized("Usuário não autenticado!");
            if (categoria.UserId != UsuarioId && categoria.UserId != null)
                return Unauthorized(new {message = "Você não tem permissão para deletar esta categoria." });

            var transacaoExists = await _transacaoService.ListTransactionsAsync(UsuarioId);
            if(transacaoExists.Any(t => t.CategoriaId == id))
                return BadRequest(new { message = "Não é possível deletar uma categoria associada a transações." });

            var result = await _categoriaService.DeleteCategoriaAsync(UsuarioId, categoria);
            if (!result)
                return BadRequest(new { message = "Não foi possível deletar a categoria." });
            return NoContent();
        }
    }
}
