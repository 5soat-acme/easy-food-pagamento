namespace EF.Pagamentos.Application.DTOs.Requests;

public class AutorizarPagamentoDto
{
    public Guid PagamentoId { get; set; }
    public bool Autorizado { get; set; }
}