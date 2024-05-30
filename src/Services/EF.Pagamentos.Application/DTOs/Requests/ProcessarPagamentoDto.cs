namespace EF.Pagamentos.Application.DTOs.Requests;

public class ProcessarPagamentoDto
{
    public Guid PedidoId { get; set; }
    public string TipoPagamento { get; set; }
    public decimal ValorTotal { get; set; }
}