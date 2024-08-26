using EF.Core.Commons.Messages.Integrations;

namespace EF.Pagamentos.Application.Events.Messages;

public class PagamentoRecusadoEvent : IntegrationEvent
{
    public Guid PedidoId { get; set; }
}