using AutoFixture.AutoMoq;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using EF.Pagamentos.Application.UseCases.Interfaces;
using EF.Api.Contexts.Pagamentos.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EF.Pagamentos.Application.DTOs.Responses;
using EF.Pagamentos.Domain.Models;
using FluentAssertions;
using EF.Core.Commons.Communication;
using EF.Pagamentos.Application.DTOs.Requests;
using EF.Api.Commons.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace EF.Api.Test.Contexts.Pagamentos.Controllers;

public class PagamentoControllerTest
{
    private readonly IFixture _fixture;
    private readonly Mock<IProcessarPagamentoUseCase> _processarPagamentoUseCaseMock;
    private readonly Mock<IAutorizarPagamentoUseCase> _autorizarPagamentoUseCaseMock;
    private readonly Mock<IConsultarPagamentoPedidoUseCase> _consultarPagamentoPedidoUseCaseMock;
    private readonly Mock<IOptions<PagamentoAutorizacaoWebHookSettings>> _pagamentoAutorizacaoWebHookSettingsMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<HttpRequest> _requestMock;
    private PagamentoController _pagamentoController;

    public PagamentoControllerTest()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _fixture.Customize<BindingInfo>(c => c.OmitAutoProperties());
        _processarPagamentoUseCaseMock = _fixture.Freeze<Mock<IProcessarPagamentoUseCase>>();
        _autorizarPagamentoUseCaseMock = _fixture.Freeze<Mock<IAutorizarPagamentoUseCase>>();
        _consultarPagamentoPedidoUseCaseMock = _fixture.Freeze<Mock<IConsultarPagamentoPedidoUseCase>>();
        _pagamentoAutorizacaoWebHookSettingsMock = _fixture.Freeze<Mock<IOptions<PagamentoAutorizacaoWebHookSettings>>>();
        _httpContextMock = _fixture.Freeze<Mock<HttpContext>>();
        _requestMock = _fixture.Freeze<Mock<HttpRequest>>();
        _pagamentoController = _fixture.Create<PagamentoController>();
    }

    [Fact]
    public async Task DeveRetornarOk_QuandoObterTiposDePagamento()
    {
        // Arrange
        var metodosPagamento = new MetodoPagamentoDto { MetodosPagamento = Enum.GetNames(typeof(Tipo)) };
        
        // Act
        var resultado = _pagamentoController.ObterTiposPagamento();

        // Assert
        var okResult = resultado.Result as OkObjectResult;
        resultado.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(metodosPagamento);
    }

    [Fact]
    public async Task DeveRetornarOk_QuandoObterPagamentoPorPedido()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var pagamentoPedidoDto = _fixture.Create<PagamentoPedidoDto>();

        _consultarPagamentoPedidoUseCaseMock.Setup(x => x.Handle(pedidoId)).ReturnsAsync(pagamentoPedidoDto);

        // Act
        var resultado = await _pagamentoController.ObterPagamentoPorPedidoId(pedidoId);

        // Assert
        var okResult = resultado as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(pagamentoPedidoDto);
    }

    [Fact]
    public async Task DeveRetornarOk_QuandoAutorizarPagamento()
    {
        // Arrange
        var operationResult = OperationResult.Success();
        var autorizarPagamentoDto = _fixture.Create<AutorizarPagamentoDto>();
        var settings = _fixture.Create<PagamentoAutorizacaoWebHookSettings>();
        settings.Key = "valid-key";

        
        _autorizarPagamentoUseCaseMock.Setup(x => x.Handle(autorizarPagamentoDto)).ReturnsAsync(operationResult);
        _pagamentoAutorizacaoWebHookSettingsMock.Setup(o => o.Value).Returns(settings);
        _requestMock.Setup(r => r.Headers["Authorization"]).Returns(new StringValues("valid-key"));
        _httpContextMock.Setup(ctx => ctx.Request).Returns(_requestMock.Object);
        _pagamentoController = _fixture.Create<PagamentoController>();
        _pagamentoController.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContextMock.Object
        };        

        // Act
        var resultado = await _pagamentoController.AutorizarPagamento(autorizarPagamentoDto);

        // Assert
        var okResult = resultado as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task DeveRetornarUnauthorized_QuandoKEyDePagamentoForInvalida()
    {
        // Arrange
        var autorizarPagamentoDto = _fixture.Create<AutorizarPagamentoDto>();
        var settings = _fixture.Create<PagamentoAutorizacaoWebHookSettings>();
        settings.Key = "valid-key";


        _pagamentoAutorizacaoWebHookSettingsMock.Setup(o => o.Value).Returns(settings);
        _requestMock.Setup(r => r.Headers["Authorization"]).Returns(new StringValues("invalid-key"));
        _httpContextMock.Setup(ctx => ctx.Request).Returns(_requestMock.Object);
        _pagamentoController = _fixture.Create<PagamentoController>();
        _pagamentoController.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContextMock.Object
        };

        // Act
        var resultado = await _pagamentoController.AutorizarPagamento(autorizarPagamentoDto);

        // Assert
        var unauthorizedResult = resultado as UnauthorizedResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoFalharAoAutorizarPagamento()
    {
        // Arrange
        var operationResult = OperationResult<Guid>.Failure("Erro ao efetuar checkout");
        var autorizarPagamentoDto = _fixture.Create<AutorizarPagamentoDto>();
        var settings = _fixture.Create<PagamentoAutorizacaoWebHookSettings>();
        settings.Key = "valid-key";


        _autorizarPagamentoUseCaseMock.Setup(x => x.Handle(autorizarPagamentoDto)).ReturnsAsync(operationResult);
        _pagamentoAutorizacaoWebHookSettingsMock.Setup(o => o.Value).Returns(settings);
        _requestMock.Setup(r => r.Headers["Authorization"]).Returns(new StringValues("valid-key"));
        _httpContextMock.Setup(ctx => ctx.Request).Returns(_requestMock.Object);
        _pagamentoController = _fixture.Create<PagamentoController>();
        _pagamentoController.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContextMock.Object
        };

        // Act
        var resultado = await _pagamentoController.AutorizarPagamento(autorizarPagamentoDto);

        // Assert
        var badRequestResult = resultado as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequestResult.Value.Should().BeEquivalentTo(new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { "Messages", operationResult.GetErrorMessages().ToArray() }
        }));
    }
}
