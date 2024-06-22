using AutoFixture.AutoMoq;
using AutoFixture;
using EF.Pagamentos.Application.UseCases;
using EF.Pagamentos.Domain.Repository;
using Moq;
using EF.Pagamentos.Domain.Models;
using FluentAssertions;
using EF.Pagamentos.Application.DTOs.Responses;

namespace EF.Pagamentos.Application.Test.UseCases;

public class ConsultarPagamentoPedidoUseCaseTest
{
    private readonly IFixture _fixture;
    private readonly Mock<IPagamentoRepository> _pagamentoRepositoryMock;
    private readonly ConsultarPagamentoPedidoUseCase _consultarPagamentoPedidoUseCase;

    public ConsultarPagamentoPedidoUseCaseTest()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _pagamentoRepositoryMock = _fixture.Freeze<Mock<IPagamentoRepository>>();
        _consultarPagamentoPedidoUseCase = _fixture.Create<ConsultarPagamentoPedidoUseCase>();
    }

    [Fact]
    public async Task DeveConsultarPagamentoPorPedidoId()
    {
        // Arrange
        var pagamento = _fixture.Create<Pagamento>();
        var pagamentoDto = new PagamentoPedidoDto()
        {
            Id = pagamento.Id,
            PedidoId = pagamento.PedidoId,
            Status = pagamento.Status
        };

        _pagamentoRepositoryMock.Setup(x => x.ObterPorPedidoId(pagamento.PedidoId)).ReturnsAsync(pagamento);

        // Act
        var resultado = await _consultarPagamentoPedidoUseCase.Handle(pagamento.PedidoId);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.ObterPorPedidoId(pagamento.PedidoId), Times.Once);
        resultado.Should().BeEquivalentTo(pagamentoDto);
    }

    [Fact]
    public async Task DeveRetornarNull_QuandoConsultarPagamentoPorPedidoInexistente()
    {
        // Arrange
        var pedidoId = _fixture.Create<Guid>();

        _pagamentoRepositoryMock.Setup(x => x.ObterPorPedidoId(pedidoId)).ReturnsAsync((Pagamento?)null);

        // Act
        var resultado = await _consultarPagamentoPedidoUseCase.Handle(pedidoId);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.ObterPorPedidoId(pedidoId), Times.Once);
        resultado.Should().BeNull();
    }
}
