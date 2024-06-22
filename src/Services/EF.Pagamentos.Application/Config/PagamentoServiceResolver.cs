using EF.Core.Commons.DomainObjects;
using EF.Pagamentos.Domain.Models;
using EF.Pagamentos.Domain.Services;
using EF.Pagamentos.Infra.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EF.Pagamentos.Application.Config;

public class PagamentoServiceResolver
{
    private readonly IServiceProvider _provider;

    public PagamentoServiceResolver(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IPagamentoService GetService(Tipo tipo)
    {
        switch (tipo)
        {
            case Tipo.MercadoPago:
                return _provider.GetRequiredService<PagamentoMercadoPagoService>();
            case Tipo.Pix:
                return _provider.GetRequiredService<PagamentoPixService>();
            case Tipo.CartaoCredito:
                return _provider.GetRequiredService<PagamentoCartaoCreditoService>();
            default:
                throw new DomainException("Tipo de pagamento inválido");
        }
    }
}