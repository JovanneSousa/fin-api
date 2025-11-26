using fin_api.Models;
using fin_api.Repositories;
using fin_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fin_api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoriesController: ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        private readonly ITransacaoRepository _transacaoService;

        public CategoriesController(ICategoriaService service, ITransacaoRepository transacaoService)
        {
            _categoriaService = service;
            _transacaoService = transacaoService;

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> ListarCategorias()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var categorias = await _categoriaService.ListCategoriasAsync(userId);
            
            return Ok(categorias);
        }


        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody]Categoria categoria)
        {


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            categoria.UserId = userId;

            try
            {
                var created = await _categoriaService.CreateCategoriaAsync(categoria);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(string id)
        {
            var categoria = await _categoriaService.GetCategoriaAsync(id);
            if (categoria == null)
                return NotFound(new { message = "Categoria não encontrada." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado." });
            if (categoria.UserId != userId || categoria.IsDefault)
                return Unauthorized(new {message = "Você não tem permissão para deletar esta categoria." });

            var transacaoExists = await _transacaoService.GetAllAsync(userId);
            if(transacaoExists.Any(t => t.CategoriaId == id))
                return BadRequest(new { message = "Não é possível deletar uma categoria associada a transações." });

            var result = await _categoriaService.DeleteCategoriaAsync(id);
            if (!result)
                return BadRequest(new { message = "Não foi possível deletar a categoria." });
            return NoContent();
        }
    }
}
