using Fin.Application.DTOs;
using Fin.Api.Extensions;
using Fin.Domain.Models;
using Fin.Application.Notificacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fin.Application.http.RequestDTO;
using Fin.Application.Interfaces.Services;

namespace Fin.Api.Controllers
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
            INotificador notificador) : base(notificador, appUser)
        {
            _transacaoService = service;
        }

        /// <summary>
        /// Obtém o saldo atual total do usuário.
        /// </summary>
        /// <returns>O valor do saldo total.</returns>
        /// <response code="200">Retorna o saldo atual.</response>
        [HttpGet("saldo")]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<decimal>> GetSaldoAtual() =>
            CustomResponse(await _transacaoService.GetSaldoTotalAsync(UsuarioId));

        /// <summary>
        /// Lista todas as transações do usuário autenticado.
        /// </summary>
        /// <returns>Uma lista de transações.</returns>
        /// <response code="200">Retorna a lista de transações.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseSuccessDTO<IEnumerable<TransacaoDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TransacaoDTO>>> GetTransacoes() =>
            CustomResponse(await _transacaoService.ListTransactionsAsync(UsuarioId));

        /// <summary>
        /// Filtra as transações por um período específico.
        /// </summary>
        /// <param name="startDate">Data de início do período.</param>
        /// <param name="endDate">Data de fim do período.</param>
        /// <returns>Uma lista de transações filtradas.</returns>
        /// <response code="200">Retorna a lista de transações filtradas.</response>
        [HttpGet("periodo")]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<IEnumerable<TransacaoDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TransacaoDTO>>> FiltrarTransacoes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
            => CustomResponse(await _transacaoService.ListTransactionsByPeriodAsync(
                UsuarioId,
                startDate,
                endDate
            ));

        /// <summary>
        /// Obtém os valores de receita e despesa agrupados por mês de um certo período
        /// </summary>
        /// <param name="startDate">Data de início do período.</param>
        /// <param name="endDate">Data de fim do período.</param>
        /// <returns>Uma lista de receita e despesas agrupadas por mês.</returns>
        /// <response code="200">Retorna a lista de receitas e despesas agrupadas.</response>
        [HttpGet("saldo-mes")]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<IEnumerable<SaldoMensalDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<SaldoMensalDTO>>> ObterReceitaDespesaPorMes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
            => CustomResponse(await _transacaoService.GetValuesByMonth(
                UsuarioId,
                startDate,
                endDate
            ));

        /// <summary>
        /// Cria uma nova transação financeira.
        /// </summary>
        /// <param name="transacao">Os dados da transação a ser criada.</param>
        /// <returns>A transação criada.</returns>
        /// <response code="200">Transação criada com sucesso.</response>
        /// <response code="400">Se houver erros nos dados enviados.</response>
        [HttpPost("novo")]
        [ClaimsAuthorize("permission", "FIN:TRN_CRIAR")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<Transacao>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransacaoDTO>> Post([FromBody] TransactionRequest transacao)
            => CustomResponse(await _transacaoService.CreateTransactionAsync(transacao, UsuarioId));

        /// <summary>
        /// Exclui uma transação específica.
        /// </summary>
        /// <param name="id">O ID da transação a ser excluída.</param>
        /// <returns>Status de sucesso.</returns>
        /// <response code="200">Transação excluída com sucesso.</response>
        /// <response code="404">Se a transação não for encontrada.</response>
        [HttpDelete("{id}")]
        [ClaimsAuthorize("permission", "FIN:TRN_EXCLUIR")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<bool>),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> Delete(string id) =>
            CustomResponse(await _transacaoService.DeleteTransactionAsync(id, UsuarioId));

        /// <summary>
        /// Obtém os detalhes de uma transação específica pelo ID.
        /// </summary>
        /// <param name="id">O ID da transação.</param>
        /// <returns>Os detalhes da transação.</returns>
        /// <response code="200">Retorna a transação solicitada.</response>
        /// <response code="404">Se a transação não for encontrada.</response>
        [HttpGet("{id}")]
        [ClaimsAuthorize("permission", "FIN:TRN_LER")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<TransacaoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransacaoDTO>> GetById(string id) =>
            CustomResponse(await _transacaoService.GetTransactionAsync(id, UsuarioId));

        /// <summary>
        /// Atualiza os dados de uma transação existente.
        /// </summary>
        /// <param name="id">O ID da transação a ser atualizada.</param>
        /// <param name="transacao">Novos dados da transação.</param>
        /// <returns>A transação atualizada.</returns>
        /// <response code="200">Transação atualizada com sucesso.</response>
        /// <response code="400">Se houver erros nos dados enviados.</response>
        [HttpPut("{id}")]
        [ClaimsAuthorize("permission", "FIN:TRN_EDITAR")]
        [ProducesResponseType(typeof(ResponseSuccessDTO<TransacaoDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseErrorDTO), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransacaoDTO>> Update(string id, [FromBody] TransacaoDTO transacao)
            => CustomResponse(await _transacaoService.UpdateTransactionAsync(id, transacao, UsuarioId));

        /// <summary>
        /// Endpoint de verificação de disponibilidade (Health Check).
        /// </summary>
        /// <returns>Status 200 OK.</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult WakeUp()
            => Ok();
    }
}
