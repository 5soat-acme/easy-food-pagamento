using EF.Api.Commons.Extensions;
using EF.Pagamentos.Application.DTOs.Requests;
using EF.Pagamentos.Application.DTOs.Responses;
using EF.Pagamentos.Application.UseCases.Interfaces;
using EF.Pagamentos.Domain.Models;
using EF.WebApi.Commons.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EF.Api.Contexts.Pagamentos.Controllers;

[Route("api/pagamentos")]
public class PagamentoController : CustomControllerBase
{
    private readonly IProcessarPagamentoUseCase _processarPagamentoUseCase;
    private readonly IAutorizarPagamentoUseCase _autorizarPagamentoUseCase;
    private readonly IConsultarPagamentoPedidoUseCase _consultarPagamentoPedidoUseCase;
    private readonly PagamentoAutorizacaoWebHookSettings _settings;

    public PagamentoController(IProcessarPagamentoUseCase processarPagamentoUseCase,
        IAutorizarPagamentoUseCase autorizarPagamentoUseCase,
        IConsultarPagamentoPedidoUseCase consultarPagamentoPedidoUseCase,
        IOptions<PagamentoAutorizacaoWebHookSettings> options)
    {
        _processarPagamentoUseCase = processarPagamentoUseCase;
        _autorizarPagamentoUseCase = autorizarPagamentoUseCase;
        _consultarPagamentoPedidoUseCase = consultarPagamentoPedidoUseCase;
        _settings = options.Value;
    }

    /// <summary>
    ///     Obtém os tipos de pagamento disponíveis.
    /// </summary>
    /// <response code="200">Retorna os tipos de pagamento.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MetodoPagamentoDto))]
    [Produces("application/json")]
    [HttpGet("tipos")]
    public ActionResult<MetodoPagamentoDto> ObterTiposPagamento()
    {
        return Ok(new MetodoPagamentoDto { MetodosPagamento = Enum.GetNames(typeof(Tipo)) });
    }

    /// <summary>
    ///     Obtém o pagamento pelo id do pedido.
    /// </summary>
    /// <response code="20o">Dados do pagamento.</response>
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [Produces("application/json")]
    [HttpGet]
    public async Task<IActionResult> ObterPagamentoPorPedidoId([FromQuery] Guid pedidoId)
    {
        return Respond(await _consultarPagamentoPedidoUseCase.Handle(pedidoId));
    }

    /// <summary>
    ///     Processa o pagamento. O pagamento é gerado e o sistema aguarda a autorização do mesmo.
    /// </summary>
    /// <response code="204">Pagamento processado.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    [HttpPost]
    public async Task<IActionResult> ProcessarPagamento(ProcessarPagamentoDto processarPagamentoDto)
    {
        var result = await _processarPagamentoUseCase.Handle(processarPagamentoDto);

        if (!result.IsValid) return Respond(result.GetErrorMessages());

        return Respond();
    }

    /// <summary>
    ///     Webhook para resposta da autorização do pagamento por parte do provedor de pagamento.
    /// </summary>
    /// <response code="204">Resposta da solicitação de autorização enviada.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    [HttpPost("autorizar/webhook")]
    public async Task<IActionResult> AutorizarPagamento(AutorizarPagamentoDto autorizarPagamentoDto)
    {
        var key = Request.Headers["Authorization"].ToString();

        if (key != _settings.Key) return Unauthorized();

        var result = await _autorizarPagamentoUseCase.Handle(autorizarPagamentoDto);

        if (!result.IsValid) return Respond(result.GetErrorMessages());

        return Respond();
    }
}