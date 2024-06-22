using EF.Core.Commons.Messages;
using EF.Infra.Commons.Messageria;
using EF.Pagamentos.Application.Events.Messages;
using EF.Pagamentos.Application.Events.Queues;
using System.Text.Json;

namespace EF.Pagamentos.Application.Events;

public class PagamentoEventHandler : IEventHandler<PagamentoAutorizadoEvent>
{
    private readonly IProducer _producer;

    public PagamentoEventHandler(IProducer producer)
    {
        _producer = producer;
    }

    public async Task Handle(PagamentoAutorizadoEvent notification)
    {
        await _producer.SendMessageAsync(QueuesNames.PagamentoAutorizado.ToString(), JsonSerializer.Serialize(notification));
    }
}
