using Amazon.SQS.Model;
using AutoFixture.AutoMoq;
using AutoFixture;
using EF.Core.Commons.Communication;
using EF.Infra.Commons.Messageria.AWS.Models;
using EF.Infra.Commons.Messageria;
using EF.Pagamentos.Application.Events.Messages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using EF.Pagamentos.Application.UseCases.Interfaces;
using EF.Pagamentos.Application.Events.Consumers;
using System.Text.Json;
using EF.Pagamentos.Application.Events.Queues;
using EF.Pagamentos.Application.DTOs.Requests;

namespace EF.Pagamentos.Application.Test.Events.Consumers;

public class PagamentoCriadoConsumerTest
{
    private readonly IFixture _fixture;
    private readonly Mock<IConsumer<AWSConsumerResponse, AwsConfirmReceipt>> _consumerMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IProcessarPagamentoUseCase> _processarPagamentoUseCase;
    private readonly PagamentoCriadoConsumer _pagamentoCriadoConsumer;

    public PagamentoCriadoConsumerTest()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _consumerMock = _fixture.Freeze<Mock<IConsumer<AWSConsumerResponse, AwsConfirmReceipt>>>();
        _serviceScopeFactoryMock = _fixture.Freeze<Mock<IServiceScopeFactory>>();
        _serviceScopeMock = _fixture.Freeze<Mock<IServiceScope>>();
        _serviceProviderMock = _fixture.Freeze<Mock<IServiceProvider>>();
        _processarPagamentoUseCase = _fixture.Freeze<Mock<IProcessarPagamentoUseCase>>();
        _pagamentoCriadoConsumer = _fixture.Create<PagamentoCriadoConsumer>();

        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IProcessarPagamentoUseCase))).Returns(_processarPagamentoUseCase.Object);
    }

    [Fact]
    public async Task DeveExecutarConsumerCompleto()
    {
        // Arrange
        var pagamentoCriadoEvent = _fixture.Create<PagamentoCriadoEvent>();
        var message = new Message
        {
            Body = JsonSerializer.Serialize(pagamentoCriadoEvent),
            ReceiptHandle = "receipt-handle"
        };

        var response = new AWSConsumerResponse
        {
            receiveMessageResponse = new ReceiveMessageResponse
            {
                Messages = new List<Message> { message }
            },
            queueUrl = "queue-url"
        };

        _consumerMock.Setup(x => x.ReceiveMessagesAsync(QueuesNames.PedidoCriado.ToString())).ReturnsAsync(response);
        _processarPagamentoUseCase.Setup(x => x.Handle(It.IsAny<ProcessarPagamentoDto>())).ReturnsAsync(It.IsAny<OperationResult<Guid>>());

        // Act
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.CancelAfter(300);
            await _pagamentoCriadoConsumer.StartAsync(cancellationTokenSource.Token);
        }

        // Assert
        _consumerMock.Verify(c => c.ReceiveMessagesAsync(It.IsAny<string>()));
        _consumerMock.Verify(c => c.ConfirmReceiptAsync(It.Is<AwsConfirmReceipt>(confirm =>
            confirm.QueueUrl == response.queueUrl &&
            confirm.ReceiptHandle == message.ReceiptHandle
        )));
        _processarPagamentoUseCase.Verify(u => u.Handle(It.Is<ProcessarPagamentoDto>(dto =>
            dto.PedidoId == pagamentoCriadoEvent.AggregateId
        )));
    }
}
