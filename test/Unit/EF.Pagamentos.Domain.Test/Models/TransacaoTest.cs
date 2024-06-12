using EF.Pagamentos.Domain.Models;
using EF.Pagamentos.Domain.Test.Fixtures;
using FluentAssertions;

namespace EF.Pagamentos.Domain.Test.Models
{
    [Collection(nameof(PagamentoCollection))]
    public class TransacaoTest(PagamentoFixture fixture)
    {
        [Fact]
        public void DeveCriarTransacao()
        {
            // Arrange
            var pagamento = fixture.GerarTransacao();

            // Act & Assert 
            pagamento.Should().BeOfType<Transacao>();
            pagamento.Data.Should().NotBe(default);
        }
    }
}
