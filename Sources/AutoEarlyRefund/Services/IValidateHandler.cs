using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Services;

public interface IValidateHandler
{
    Task<bool> HandleAsync(Match match);
}


public class DefaultValidateHandler : IValidateHandler
{
    public async Task<bool> HandleAsync(Match match)
    {
        return true;
    }
}