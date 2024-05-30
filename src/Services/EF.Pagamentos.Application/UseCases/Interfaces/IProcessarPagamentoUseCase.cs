using EF.Core.Commons.Communication;
using EF.Pagamentos.Application.DTOs.Requests;

namespace EF.Pagamentos.Application.UseCases.Interfaces;

public interface IProcessarPagamentoUseCase
{
    Task<OperationResult> Handle(ProcessarPagamentoDto processarPagamentoDto);
}