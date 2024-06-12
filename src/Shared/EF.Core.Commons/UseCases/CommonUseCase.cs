using EF.Core.Commons.Communication;
using EF.Core.Commons.Repository;

namespace EF.Core.Commons.UseCases;

public abstract class CommonUseCase
{
    protected ValidationResult ValidationResult = new();

    protected void AddError(string message, string propertyName = "")
    {
        ValidationResult.AddError(message, propertyName);
    }
}