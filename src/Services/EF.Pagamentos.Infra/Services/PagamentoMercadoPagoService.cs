using EF.Pagamentos.Domain.Models;
using EF.Pagamentos.Domain.Services;

namespace EF.Pagamentos.Infra.Services;

public class PagamentoMercadoPagoService : IPagamentoService
{
    public Task AutorizarPagamento(Pagamento pagamento)
    {
        return Task.CompletedTask;
    }
}