using fin_api.Extensions;
using fin_api.Models;
using fin_api.Notificacoes;
using fin_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace fin_api.Controllers
{
    [ApiController]
    [Route("api/transacoes")]
    [Authorize]
    public class TransacoesController : MainController
    {
        private readonly ITransacaoService _transacaoService;

        public TransacoesController(
            ITransacaoService service, 
            IUser appUser, 
            INotificador notificador) : base (notificador, appUser)
        {
            _transacaoService = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transacao>>> GetTransacoes() =>
            CustomResponse(await _transacaoService.ListTransactionsAsync(UsuarioId));

        [HttpGet("periodo")]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        public async Task<ActionResult<IEnumerable<Transacao>>> FiltrarTransacoes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
            => CustomResponse(await _transacaoService.ListTransactionsByPeriodAsync(
                UsuarioId,
                startDate,
                endDate
            ));

        [HttpPost("novo")]
        [ClaimsAuthorize("permission", "FIN:TRN_CRIAR")]
        public async Task<IActionResult> Post([FromBody] Transacao transacao)
            => CustomResponse(await _transacaoService.CreateTransactionAsync(transacao, UsuarioId));

        [HttpDelete("{id}")]
        [ClaimsAuthorize("permission", "FIN:TRN_EXCLUIR")]
        public async Task<IActionResult> Delete(string id) =>
            CustomResponse(await _transacaoService.DeleteTransactionAsync(id, UsuarioId));

        [HttpGet("{id}")]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        public async Task<ActionResult<Transacao>> GetById(string id) =>
            CustomResponse(await _transacaoService.GetTransactionAsync(id, UsuarioId));

        [HttpPut("{id}")]
        [ClaimsAuthorize("permission", "FIN:TRN_EDITAR")]
        public async Task<ActionResult<Transacao>> Update(string id, [FromBody] Transacao transacao)
            => CustomResponse(await _transacaoService.UpdateTransactionAsync(id, transacao, UsuarioId));
    }
}
