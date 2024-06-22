using EF.Core.Commons.UseCases;
using EF.Pagamentos.Application.DTOs.Responses;
using EF.Pagamentos.Application.UseCases.Interfaces;
using EF.Pagamentos.Domain.Repository;

namespace EF.Pagamentos.Application.UseCases;

public class ConsultarPagamentoPedidoUseCase : CommonUseCase, IConsultarPagamentoPedidoUseCase
{
    private readonly IPagamentoRepository _pagamentoRepository;

    public ConsultarPagamentoPedidoUseCase(IPagamentoRepository pagamentoRepository)
    {
        _pagamentoRepository = pagamentoRepository;
    }

    public async Task<PagamentoPedidoDto?> Handle(Guid pedidoId)
    {
        PagamentoPedidoDto? result = null;
        var pagamento = await _pagamentoRepository.ObterPorPedidoId(pedidoId);

        if (pagamento is not null)
        {
            result = new PagamentoPedidoDto
            {
                Id = pagamento.Id,
                PedidoId = pagamento.PedidoId,
                Status = pagamento.Status
            };
        }

        return result;
    }
}