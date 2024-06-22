using EF.Pagamentos.Domain.Models;

namespace EF.Pagamentos.Domain.Services;

public interface IPagamentoService
{
    public Task AutorizarPagamento(Pagamento pagamento);
}