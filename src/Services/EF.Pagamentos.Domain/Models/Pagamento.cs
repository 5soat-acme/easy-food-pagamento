using EF.Core.Commons.DomainObjects;
using System.Data;

namespace EF.Pagamentos.Domain.Models;

public class Pagamento : Entity, IAggregateRoot
{
    private List<Transacao> _transacoes;

    private Pagamento()
    {
    }

    public Pagamento(Guid pedidoId, Tipo tipo, decimal valor)
    {
        if (!ValidarPedido(pedidoId)) throw new DomainException("Um pagamento deve estar associado a um pedido");
        if (!ValidarValor(valor)) throw new DomainException("Valor inválido");

        PedidoId = pedidoId;
        Tipo = tipo;
        Valor = valor;
        DataCriacao = DateTime.Now;
        _transacoes = new List<Transacao>();
        Status = Models.Status.Pendente;
    }

    public Pagamento(Guid id, Guid pedidoId, Tipo tipo, DateTime dataCriacao, DateTime? dataAtualizacao, decimal valor, Status status)
    {
        if (!ValidarPedido(pedidoId)) throw new DomainException("Um pagamento deve estar associado a um pedido");
        if (!ValidarValor(valor)) throw new DomainException("Valor inválido");

        Id = id;
        PedidoId = pedidoId;
        Tipo = tipo;
        DataCriacao = dataCriacao;
        DataAtualizacao = dataAtualizacao;
        Valor = valor;
        Status = status;
        _transacoes = new List<Transacao>();
    }

    public Guid PedidoId { get; private set; }
    public Tipo Tipo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public decimal Valor { get; private set; }
    public Status Status { get; private set; }
    public IReadOnlyCollection<Transacao> Transacoes => _transacoes;

    private bool ValidarPedido(Guid pedidoId)
    {
        if (pedidoId == Guid.Empty) return false;

        return true;
    }

    private bool ValidarValor(decimal valor)
    {
        if (valor <= 0) return false;

        return true;
    }

    public void AdicionarTransacao(Transacao transacao)
    {
        _transacoes.Add(transacao);
    }

    public void Autorizar()
    {
        Status = Status.Autorizado;
        DataAtualizacao = DateTime.Now;
    }

    public void Recusar()
    {
        Status = Status.Recusado;
        DataAtualizacao = DateTime.Now;
    }
}