using Fin.Api.Extensions;
using Fin.Application.DTOs;
using Fin.Application.Interfaces.Services;
using Fin.Application.Notificacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fin.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioController : MainController
    {
        private readonly IUsuarioService _usuarioService;
        public UsuarioController(
            IUsuarioService usuarioService,
            INotificador notificador, 
            IUser appUser) : base(notificador, appUser)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Obtém os dados do perfil do usuário autenticado.
        /// </summary>
        /// <returns>Os dados do usuário.</returns>
        /// <response code="200">Retorna o perfil do usuário.</response>
        /// <response code="404">Se o usuário não for encontrado.</response>
        [HttpGet]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<UsuarioDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario()
            => CustomResponse(await _usuarioService.BuscarUsuarioPorIdAsync(UsuarioId));
    }
}
