
using AutoRefund.Core.Models;
using AutoRefundFM.Models;

namespace AutoRefundFM;

public interface IValidateHandler
{
    Task<Result> HandleAsync(Match match);
}

public class DefaultValidateHandler : IValidateHandler
{
    public Task<Result> HandleAsync(Match match)
    {
        return Task.FromResult(new Result());
    }
}