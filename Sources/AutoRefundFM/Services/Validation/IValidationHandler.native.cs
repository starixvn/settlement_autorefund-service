using AutoRefund.Core.Models;

namespace AutoRefundFM.Services.Validation;

public interface IValidationHandler
{
    Task<Result> Handle(ValidationContext validationContext);
}

public abstract class ValidationHandlerBase(IValidationHandler? next = null) : IValidationHandler
{
    public async Task<Result> Handle(ValidationContext validationContext)
    {
        var result = await ProcessAsync(validationContext);
        if (!result.IsSuccess)
        {
            return result;
        }

        if (next != null && result.IsSuccess)
        {
            return await next.Handle(validationContext);
        }

        return new Result();
    }

    protected abstract Task<Result> ProcessAsync(ValidationContext validationContext);
}

public class DefaultValidationHandler : ValidationHandlerBase
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        return await Task.FromResult(new Result());
    }
}

