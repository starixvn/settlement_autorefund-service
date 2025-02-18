
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Services;

public interface IValidateHandler
{
    Task<Result> HandleAsync(Match match);
}


public class DefaultValidateHandler : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {
        return new Result();
    }
}