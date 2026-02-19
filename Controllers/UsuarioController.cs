using fin_api.DTOs;
using fin_api.Extensions;
using fin_api.Notificacoes;
using fin_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fin_api.Controllers
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

        [HttpGet]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        public async Task<ActionResult<UsuarioDTO>> GetUsuario()
            => await _usuarioService.BuscarUsuarioPorIdAsync(UsuarioId);
    }
}
