using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Services;

public interface IValidateHandler
{
    Task<bool> HandleAsync(Match match);
}
