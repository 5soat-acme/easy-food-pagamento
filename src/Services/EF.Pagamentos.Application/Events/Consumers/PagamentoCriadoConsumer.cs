using EF.Infra.Commons.Messageria.AWS.Models;
using EF.Infra.Commons.Messageria;
using EF.Pagamentos.Application.Events.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using EF.Pagamentos.Application.UseCases.Interfaces;
using EF.Pagamentos.Application.Events.Messages;
using EF.Pagamentos.Application.DTOs.Requests;

namespace EF.Pagamentos.Application.Events.Consumers;

public class PagamentoCriadoConsumer : BackgroundService
{
    private readonly IConsumer<AWSConsumerResponse, AwsConfirmReceipt> _consumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PagamentoCriadoConsumer(IConsumer<AWSConsumerResponse, AwsConfirmReceipt> consumer,
                        IServiceScopeFactory serviceScopeFactory)
    {
        _consumer = consumer;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _consumer.ReceiveMessagesAsync(QueuesNames.PedidoCriado.ToString());

                foreach (var message in response.receiveMessageResponse.Messages)
                {
                    using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                    {
                        var processarPagamentoUseCase = scope.ServiceProvider.GetRequiredService<IProcessarPagamentoUseCase>();
                        var pedidoCriado = JsonSerializer.Deserialize<PagamentoCriadoEvent>(message.Body);

                        if (pedidoCriado != null)
                        {
                            await processarPagamentoUseCase.Handle(new ProcessarPagamentoDto
                            {
                                PedidoId = pedidoCriado.AggregateId,
                                TipoPagamento = pedidoCriado.TipoPagamento,
                                ValorTotal = pedidoCriado.ValorTotal
                            });

                            var confirm = new AwsConfirmReceipt
                            {
                                QueueUrl = response.queueUrl,
                                ReceiptHandle = message.ReceiptHandle
                            };

                            await _consumer.ConfirmReceiptAsync(confirm);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
    }
}
