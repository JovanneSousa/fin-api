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
        public async Task<ActionResult<IEnumerable<Transacao>>> GetTransacoes()
        {

            if (!UsuarioAutenticado)
                return Unauthorized("Usuário não autenticado");

            var transacoes = await _transacaoService.ListTransactionsAsync(UsuarioId);
            return Ok(transacoes);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult<IEnumerable<Transacao>>> FiltrarTransacoes(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (!UsuarioAutenticado)
                return Unauthorized("Usuário não autenticado");

            var startLocal = startDate.Date;
            var endLocal = endDate.Date.AddDays(1).AddTicks(-1);

            var inicioUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal);
            var fimUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal);

            var transacoes = await _transacaoService.ListTransactionsByPeriodAsync(
                UsuarioId,
                inicioUtc,
                fimUtc
            );
            if (transacoes == null) return BadRequest(new { message = "Falha ao buscar os dados" });

            return Ok(transacoes);
        }

        [HttpPost("novo")]
        public async Task<IActionResult> Post([FromBody] Transacao transacao)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!UsuarioAutenticado)
                return Unauthorized("Usuário não autenticado");

            transacao.DataMovimentacao = 
                DateTime.SpecifyKind(transacao.DataMovimentacao, DateTimeKind.Utc);
            transacao.UserId = UsuarioId;
            var result = await _transacaoService.CreateTransactionAsync(transacao);

            if (result == null)
                return BadRequest("Erro ao criar a transação.");

            return Ok(transacao);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!UsuarioAutenticado)
                return Unauthorized("Usuário não autenticado");

            var transacao = await _transacaoService.GetTransactionAsync(id);
            if (transacao.Id == null || transacao.UserId != UsuarioId)
                return NotFound("Transação não encontrada.");

            var success = await _transacaoService.DeleteTransactionAsync(id);
            if (!success)
                return NotFound("Falha ao excluir a transação");
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transacao>> GetById(string id)
        {
            if (!UsuarioAutenticado)
                return Unauthorized("Usuário não autenticado");

            var transacao = await _transacaoService.GetTransactionAsync(id);
            if (transacao.UserId != UsuarioId)
                return Unauthorized(new { message = "Você não tem permissão para excluir essa tarefa" });

            if (transacao == null)
                return NotFound(new { message = "Transação não encontrada" });

            return Ok(transacao);


        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Transacao>> Update(string id, [FromBody] Transacao transacao)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!UsuarioAutenticado)
                return Unauthorized("Usuário não autenticado");

            var existing = await _transacaoService.GetTransactionAsync(id);
            if (existing == null)
                return NotFound(new { message = "Transação não encontrada." });

            if (existing.UserId != UsuarioId)
                return Unauthorized(new { message = "Você não tem permissão para atualizar essa transação." });

            transacao.Id = existing.Id;
            transacao.UserId = existing.UserId;
            transacao.Type = existing.Type;
            transacao.CreatedAt = existing.CreatedAt;
            transacao.ParentTransactionId = existing.ParentTransactionId;

            var updated = await _transacaoService.UpdateTransactionAsync(transacao);

            if (updated == null)
                return BadRequest(new { message = "Erro ao atualizar a transação." });

            return Ok(updated);
        }
    }
}
