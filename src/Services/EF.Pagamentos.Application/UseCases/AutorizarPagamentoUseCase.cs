using EF.Core.Commons.Communication;
using EF.Core.Commons.UseCases;
using EF.Pagamentos.Application.DTOs.Requests;
using EF.Pagamentos.Application.Events.Messages;
using EF.Pagamentos.Application.UseCases.Interfaces;
using EF.Pagamentos.Domain.Models;
using EF.Pagamentos.Domain.Repository;

namespace EF.Pagamentos.Application.UseCases;

public class AutorizarPagamentoUseCase : CommonUseCase, IAutorizarPagamentoUseCase
{
    private readonly IPagamentoRepository _pagamentoRepository;

    public AutorizarPagamentoUseCase(IPagamentoRepository pagamentoRepository)
    {
        _pagamentoRepository = pagamentoRepository;
    }

    public async Task<OperationResult> Handle(AutorizarPagamentoDto autorizarPagamentoDto)
    {
        var pagamento = await _pagamentoRepository.ObterPorId(autorizarPagamentoDto.PagamentoId);

        if (pagamento is null)
        {
            AddError("Pagamento inválido", "Pagamento");
            return OperationResult.Failure(ValidationResult);
        }

        if (autorizarPagamentoDto.Autorizado)
        {
            var transacao = new Transacao(pagamento.Id);
            await _pagamentoRepository.AdicionarTransacao(transacao);
            pagamento.Autorizar();
            pagamento.AdicionarTransacao(transacao);
            pagamento.AddEvent(new PagamentoAutorizadoEvent
            {
                AggregateId = pagamento.Id,
                PedidoId = pagamento.PedidoId
            });
        }
        else
        {
            pagamento.Recusar();
        }

        await _pagamentoRepository.Atualizar(pagamento);

        return OperationResult.Success();
    }
}