using EF.Core.Commons.DomainObjects;

namespace EF.Pagamentos.Domain.Models;

public class Transacao : Entity
{
    public Transacao(Guid pagamentoId)
    {
        PagamentoId = pagamentoId;
        Data = DateTime.Now.ToUniversalTime();
    }

    public Guid PagamentoId { get; private set; }
    public DateTime Data { get; private set; }

    public Pagamento Pagamento { get; private set; }
}