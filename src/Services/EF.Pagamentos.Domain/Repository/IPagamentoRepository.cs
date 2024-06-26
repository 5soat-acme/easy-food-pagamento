﻿using EF.Pagamentos.Domain.Models;

namespace EF.Pagamentos.Domain.Repository;

public interface IPagamentoRepository : IDisposable
{
    Task<Pagamento?> ObterPorId(Guid id);
    Task<Pagamento?> ObterPorPedidoId(Guid pedidoId);
    Task Criar(Pagamento pagamento);
    Task Atualizar(Pagamento pagamento);
    Task AdicionarTransacao(Transacao transacao);
}