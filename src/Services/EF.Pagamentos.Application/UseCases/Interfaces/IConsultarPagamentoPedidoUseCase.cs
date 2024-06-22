using EF.Pagamentos.Application.DTOs.Responses;

namespace EF.Pagamentos.Application.UseCases.Interfaces;

public interface IConsultarPagamentoPedidoUseCase
{
    Task<PagamentoPedidoDto?> Handle(Guid pedidoId);
}