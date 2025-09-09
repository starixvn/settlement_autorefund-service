using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;

public interface IValidationHandler
{
    Task<Result> Handle(SpecialSession session);
}


public abstract class ValidationHandlerBase(IValidationHandler? next = null) : IValidationHandler
{
    public async Task<Result> Handle(SpecialSession session)
    {
        var result = await ProcessAsync(session);
        if (!result.IsSuccess)
        {
            return result;
        }

        if (next != null && result.IsSuccess)
        {
            return await next.Handle(session);
        }

        return new Result();
    }

    protected abstract Task<Result> ProcessAsync(SpecialSession session);
}

public class DefaultValidationHandler : ValidationHandlerBase
{
    protected override async Task<Result> ProcessAsync(SpecialSession session)
    {
        return await Task.FromResult(new Result());
    }
}

