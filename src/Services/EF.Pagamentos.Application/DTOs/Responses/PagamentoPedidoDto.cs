using EF.Pagamentos.Domain.Models;

namespace EF.Pagamentos.Application.DTOs.Responses;

public class PagamentoPedidoDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Status Status { get; set; }
}