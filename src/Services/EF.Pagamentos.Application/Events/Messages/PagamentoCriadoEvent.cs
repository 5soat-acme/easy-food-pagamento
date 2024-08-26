using EF.Core.Commons.Messages.Integrations;

namespace EF.Pagamentos.Application.Events.Messages;

public class PagamentoCriadoEvent : IntegrationEvent
{
    public string TipoPagamento { get; set; }
    public decimal ValorTotal { get; set; }
}