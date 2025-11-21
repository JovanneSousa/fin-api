using fin_api.Models;
using fin_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fin_api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController: ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriesController(ICategoriaService service)
        {
            _categoriaService = service;
            
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



    }
}
