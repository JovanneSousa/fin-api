using fin_api.Models;
using fin_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace fin_api.Controllers
{
    [ApiController]
    [Route("api/transacoes")]
    [Authorize]
    public class TransacoesController : ControllerBase
    {
        private readonly ITransacaoService _transacaoService;

        public TransacoesController(ITransacaoService service)
        {
            _transacaoService = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transacao>>> GetTransacoes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var transacoes = await _transacaoService.ListTransactionsAsync(userId);
            return Ok(transacoes);
        }

        [HttpPost("novo")]
        public async Task<IActionResult> Post([FromBody] Transacao transacao)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            transacao.UserId = userId;
            var result = await _transacaoService.CreateTransactionAsync(transacao);

            if (result == null)
                return BadRequest("Erro ao criar a transação.");

            return Ok(transacao);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Usuário não autenticado.");

            var transacao = await _transacaoService.GetTransactionAsync(id);
            if(transacao.Id == null || transacao.UserId != userId)
                return NotFound("Transação não encontrada.");

            var success = await _transacaoService.DeleteTransactionAsync(id);
            if (!success)
                return NotFound("Transação não encontrada.");
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Transacao>> GetById(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuário não autenticado" });

            var transacao = await _transacaoService.GetTransactionAsync(id);
            if (transacao.UserId != userId) 
                return Unauthorized(new { message = "Você não tem permissão para excluir essa tarefa" });

            if (transacao == null) 
                return NotFound(new { message = "Transação não encontrada" });

            return Ok(transacao);


        }
    }
}
