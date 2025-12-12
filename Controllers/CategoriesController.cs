using fin_api.Extensions;
using fin_api.Models;
using fin_api.Notificacoes;
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

        public CategoriesController(
            ICategoriaService service,
            INotificador notificador,
            IUser appUser) : base (notificador, appUser)
        {
            _categoriaService = service;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> ListarCategorias() 
            => CustomResponse(await _categoriaService.ListCategoriasAsync(UsuarioId));

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] Categoria categoria)
            => CustomResponse(await _categoriaService.CreateCategoriaAsync(UsuarioId, categoria));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(string id)
            => CustomResponse(!await _categoriaService.DeleteCategoriaAsync(UsuarioId, id));
    }
}
