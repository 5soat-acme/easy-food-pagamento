using AutoFixture.AutoMoq;
using AutoFixture;
using Moq;
using EF.Infra.Commons.Messageria;
using EF.Pagamentos.Application.Events;
using EF.Pagamentos.Application.Events.Queues;
using EF.Pagamentos.Application.Events.Messages;
using System.Text.Json;

namespace EF.Pagamentos.Application.Test.Events;

public class PagamentoEventHandlerTest
{
    private readonly IFixture _fixture;
    private readonly Mock<IProducer> _producerMock;
    private readonly PagamentoEventHandler _pagamentoEventHandler;

    public PagamentoEventHandlerTest()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _producerMock = _fixture.Freeze<Mock<IProducer>>();
        _pagamentoEventHandler = _fixture.Create<PagamentoEventHandler>();
    }

    [Fact]
    public async Task DeveExecutarEventoPagamentoAutorizado()
    {
        // Arrange
        var pagamentoAutorizadoEvent = _fixture.Create<PagamentoAutorizadoEvent>();
        var pagamentoAutorizadoEventJson = JsonSerializer.Serialize(pagamentoAutorizadoEvent);

        _producerMock.Setup(x => x.SendMessageAsync(QueuesNames.PagamentoAutorizado.ToString(), pagamentoAutorizadoEventJson));

        // Act
        await _pagamentoEventHandler.Handle(pagamentoAutorizadoEvent);

        // Assert
        _producerMock.Verify(x => x.SendMessageAsync(QueuesNames.PagamentoAutorizado.ToString(), pagamentoAutorizadoEventJson), Times.Once);
    }

    [Fact]
    public async Task DeveExecutarEventoPagamentoRecusado()
    {
        // Arrange
        var pagamentoRecusadoEvent = _fixture.Create<PagamentoRecusadoEvent>();
        var pagamentoRecusadoEventJson = JsonSerializer.Serialize(pagamentoRecusadoEvent);

        _producerMock.Setup(x => x.SendMessageAsync(QueuesNames.PagamentoRecusado.ToString(), pagamentoRecusadoEventJson));

        // Act
        await _pagamentoEventHandler.Handle(pagamentoRecusadoEvent);

        // Assert
        _producerMock.Verify(x => x.SendMessageAsync(QueuesNames.PagamentoRecusado.ToString(), pagamentoRecusadoEventJson), Times.Once);
    }
}
