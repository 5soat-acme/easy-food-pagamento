using EF.Core.Commons.DomainObjects;
using EF.Pagamentos.Domain.Models;
using EF.Pagamentos.Domain.Test.Fixtures;
using FluentAssertions;

namespace EF.Pagamentos.Domain.Test.Models
{
    [Collection(nameof(PagamentoCollection))]
    public class PagamentoTest(PagamentoFixture fixture)
    {
        [Fact]
        public void DeveCriarPagamento()
        {
            // Arrange
            var pagamento = fixture.GerarPagamento();

            // Act & Assert 
            pagamento.Should().BeOfType<Pagamento>();
            pagamento.Status.Should().Be(Status.Pendente);
        }

        [Fact]
        public void DeveGerarExcecao_QuandoCriarPagamentoComPedidoInvalido()
        {
            // Arrange - Act
            Action act = () => fixture.GerarPagamento(pedidoId: Guid.Empty);

            // Assert
            act.Should().Throw<DomainException>().WithMessage("Um pagamento deve estar associado a um pedido");
        }

        [Fact]
        public void DeveGerarExcecao_QuandoCriarPagamentoComValorInvalido()
        {
            // Arrange - Act
            Action act = () => fixture.GerarPagamento(valor: 0);

            // Assert
            act.Should().Throw<DomainException>().WithMessage("Valor inválido");
        }

        [Fact]
        public void DeveAutorizarPagamento()
        {
            // Arrange
            var pagamento = fixture.GerarPagamento();

            // Act
            pagamento.Autorizar();

            // Assert
            pagamento.Status.Should().Be(Status.Autorizado);
        }

        [Fact]
        public void DeveRecusarPagamento()
        {
            // Arrange
            var pagamento = fixture.GerarPagamento();

            // Act
            pagamento.Recusar();

            // Assert
            pagamento.Status.Should().Be(Status.Recusado);
        }

        [Fact]
        public void DeveAdicionarTeansacaoAoPagamento()
        {
            // Arrange
            var pagamento = fixture.GerarPagamento();
            var transacao = fixture.GerarTransacao(pagamentoId: pagamento.Id);

            // Act
            pagamento.AdicionarTransacao(transacao);

            // Assert
            pagamento.Transacoes.Should().Contain(transacao);
        }
    }
}
