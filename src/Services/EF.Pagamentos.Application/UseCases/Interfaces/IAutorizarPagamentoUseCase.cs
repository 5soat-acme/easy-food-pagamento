using EF.Core.Commons.Communication;
using EF.Pagamentos.Application.DTOs.Requests;

namespace EF.Pagamentos.Application.UseCases.Interfaces;

public interface IAutorizarPagamentoUseCase
{
    Task<OperationResult> Handle(AutorizarPagamentoDto autorizarPagamentoDto);
}