using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Services;

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