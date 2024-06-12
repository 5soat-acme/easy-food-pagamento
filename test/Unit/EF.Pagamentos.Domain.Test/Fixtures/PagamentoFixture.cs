using Bogus;
using EF.Pagamentos.Domain.Models;
using EF.Test.Utils;

namespace EF.Pagamentos.Domain.Test.Fixtures
{
    [CollectionDefinition(nameof(PagamentoCollection))]
    public class PagamentoCollection : ICollectionFixture<PagamentoFixture>
    {
    }

    public class PagamentoFixture
    {
        public Pagamento GerarPagamento(Guid? pedidoId = null, Tipo? tipo = null, decimal? valor = null)
        {
            var pagamento = new Faker<Pagamento>("pt_BR")
                .CustomInstantiator(f => new Pagamento(pedidoId ?? Guid.NewGuid(), tipo ?? UtilsTest.GetRandomEnum<Tipo>(Enum.GetValues(typeof(Tipo))), valor ?? f.Random.Decimal(1, 20)));

            return pagamento.Generate();
        }

        public Transacao GerarTransacao(Guid? pagamentoId = null)
        {
            var transacao = new Faker<Transacao>("pt_BR")
                .CustomInstantiator(f => new Transacao(pagamentoId ?? Guid.NewGuid()));

            return transacao.Generate();
        }
    }
}
