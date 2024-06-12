using AutoFixture.AutoMoq;
using AutoFixture;
using Moq;
using EF.Pagamentos.Domain.Repository;
using EF.Pagamentos.Application.UseCases;
using EF.Pagamentos.Application.DTOs.Requests;
using EF.Pagamentos.Domain.Models;
using FluentAssertions;
using EF.Core.Commons.DomainObjects;

namespace EF.Pagamentos.Application.Test.UseCases;

public class AutorizarPagamentoUseCaseTest
{
    private readonly IFixture _fixture;
    private readonly Mock<IPagamentoRepository> _pagamentoRepositoryMock;
    private readonly AutorizarPagamentoUseCase _autorizarPagamentoUseCase;

    public AutorizarPagamentoUseCaseTest()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _pagamentoRepositoryMock = _fixture.Freeze<Mock<IPagamentoRepository>>();
        _autorizarPagamentoUseCase = _fixture.Create<AutorizarPagamentoUseCase>();
    }

    [Fact]
    public async Task DeveAutorizarPagamento()
    {
        // Arrange
        var autorizarPagamentoDto = _fixture.Build<AutorizarPagamentoDto>().With(x => x.Autorizado, true).Create();
        var pagamento = _fixture.Create<Pagamento>();

        _pagamentoRepositoryMock.Setup(x => x.ObterPorId(autorizarPagamentoDto.PagamentoId)).ReturnsAsync(pagamento);
        _pagamentoRepositoryMock.Setup(x => x.AdicionarTransacao(It.IsAny<Transacao>()));
        _pagamentoRepositoryMock.Setup(x => x.Atualizar(pagamento));

        // Act
        var resultado = await _autorizarPagamentoUseCase.Handle(autorizarPagamentoDto);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.ObterPorId(autorizarPagamentoDto.PagamentoId), Times.Once);
        _pagamentoRepositoryMock.Verify(x => x.AdicionarTransacao(It.IsAny<Transacao>()), Times.Once);
        _pagamentoRepositoryMock.Verify(x => x.Atualizar(pagamento), Times.Once);
        resultado.IsValid.Should().BeTrue();
        resultado.ValidationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DeveGerarExcecao_QuandoPagamentoForInvalido()
    {
        // Arrange
        var autorizarPagamentoDto = _fixture.Create<AutorizarPagamentoDto>();

        _pagamentoRepositoryMock.Setup(x => x.ObterPorId(autorizarPagamentoDto.PagamentoId)).ReturnsAsync((Pagamento?)null);

        // Act
        var resultado = await _autorizarPagamentoUseCase.Handle(autorizarPagamentoDto);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.ObterPorId(autorizarPagamentoDto.PagamentoId), Times.Once);
        _pagamentoRepositoryMock.Verify(x => x.AdicionarTransacao(It.IsAny<Transacao>()), Times.Never);
        _pagamentoRepositoryMock.Verify(x => x.Atualizar(It.IsAny<Pagamento>()), Times.Never);
        resultado.IsValid.Should().BeFalse();
        resultado.GetErrorMessages().Count(x => x == "Pagamento inválido").Should().Be(1);
    }

    [Fact]
    public async Task DeveRecusarPagamento()
    {
        // Arrange
        var autorizarPagamentoDto = _fixture.Build<AutorizarPagamentoDto>().With(x => x.Autorizado, false).Create();
        var pagamento = _fixture.Create<Pagamento>();

        _pagamentoRepositoryMock.Setup(x => x.ObterPorId(autorizarPagamentoDto.PagamentoId)).ReturnsAsync(pagamento);
        _pagamentoRepositoryMock.Setup(x => x.Atualizar(pagamento));

        // Act
        var resultado = await _autorizarPagamentoUseCase.Handle(autorizarPagamentoDto);

        // Assert
        _pagamentoRepositoryMock.Verify(x => x.ObterPorId(autorizarPagamentoDto.PagamentoId), Times.Once);
        _pagamentoRepositoryMock.Verify(x => x.AdicionarTransacao(It.IsAny<Transacao>()), Times.Never);
        _pagamentoRepositoryMock.Verify(x => x.Atualizar(pagamento), Times.Once);
        resultado.IsValid.Should().BeTrue();
        resultado.ValidationResult.IsValid.Should().BeTrue();
    }
}
