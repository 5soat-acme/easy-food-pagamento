using AutoFixture.AutoMoq;
using AutoFixture;
using EF.Pagamentos.Application.UseCases;
using EF.Pagamentos.Domain.Repository;
using Moq;
using EF.Pagamentos.Application.Config;
using EF.Pagamentos.Application.DTOs.Requests;
using EF.Pagamentos.Domain.Models;
using FluentAssertions;
using EF.Pagamentos.Infra.Services;
using EF.Core.Commons.DomainObjects;

namespace EF.Pagamentos.Application.Test.UseCases;

public class ProcessarPagamentoUseCaseTest
{
    private readonly IFixture _fixture;
    private readonly Mock<IPagamentoRepository> _pagamentoRepositoryMock;
    private readonly ProcessarPagamentoUseCase _processarPagamentoUseCase;

    public ProcessarPagamentoUseCaseTest()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());

        var mockProvider = _fixture.Freeze<Mock<IServiceProvider>>();
        mockProvider.Setup(p => p.GetService(typeof(PagamentoMercadoPagoService)))
                    .Returns(new PagamentoMercadoPagoService());

        mockProvider.Setup(p => p.GetService(typeof(PagamentoPixService)))
                    .Returns(new PagamentoPixService());

        mockProvider.Setup(p => p.GetService(typeof(PagamentoCartaoCreditoService)))
                    .Returns(new PagamentoCartaoCreditoService());

        _pagamentoRepositoryMock = _fixture.Freeze<Mock<IPagamentoRepository>>();
        _fixture.Create<PagamentoServiceResolver>();
        _processarPagamentoUseCase = _fixture.Create<ProcessarPagamentoUseCase>();
    }

    [Fact]
    public async Task DeveProcessarPagamentoPix()
    {
        // Arrange
        var processarPagamentoDto = _fixture.Build<ProcessarPagamentoDto>().With(x => x.TipoPagamento, Tipo.Pix.ToString()).Create();

        _pagamentoRepositoryMock.Setup(x => x.Criar(It.IsAny<Pagamento>()));

        // Act
        var resultado = await _processarPagamentoUseCase.Handle(processarPagamentoDto);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.Criar(It.IsAny<Pagamento>()), Times.Once);
        resultado.IsValid.Should().BeTrue();
        resultado.ValidationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DeveProcessarPagamentoMercadoPago()
    {
        // Arrange
        var processarPagamentoDto = _fixture.Build<ProcessarPagamentoDto>().With(x => x.TipoPagamento, Tipo.MercadoPago.ToString()).Create();

        _pagamentoRepositoryMock.Setup(x => x.Criar(It.IsAny<Pagamento>()));

        // Act
        var resultado = await _processarPagamentoUseCase.Handle(processarPagamentoDto);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.Criar(It.IsAny<Pagamento>()), Times.Once);
        resultado.IsValid.Should().BeTrue();
        resultado.ValidationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DeveProcessarPagamentoCartaoCredito()
    {
        // Arrange
        var processarPagamentoDto = _fixture.Build<ProcessarPagamentoDto>().With(x => x.TipoPagamento, Tipo.CartaoCredito.ToString()).Create();

        _pagamentoRepositoryMock.Setup(x => x.Criar(It.IsAny<Pagamento>()));

        // Act
        var resultado = await _processarPagamentoUseCase.Handle(processarPagamentoDto);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.Criar(It.IsAny<Pagamento>()), Times.Once);
        resultado.IsValid.Should().BeTrue();
        resultado.ValidationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DeveGerarExccessao_QuandoProcessarPagamentoComTipoInvalido()
    {
        // Arrange
        var processarPagamentoDto = _fixture.Build<ProcessarPagamentoDto>().With(x => x.TipoPagamento, "invalido").Create();

        _pagamentoRepositoryMock.Setup(x => x.Criar(It.IsAny<Pagamento>()));

        // Act
        Func<Task> act = async () => await _processarPagamentoUseCase.Handle(processarPagamentoDto);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.Criar(It.IsAny<Pagamento>()), Times.Never);
        await act.Should().ThrowAsync<DomainException>().WithMessage("Tipo de pagamento inválido");
    }
}