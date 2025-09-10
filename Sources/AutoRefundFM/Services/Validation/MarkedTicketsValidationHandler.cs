using AutoRefund.Core.Models;
using AutoRefundFM.Features.CheckMarkedTicket;

namespace AutoRefundFM.Services.Validation;

public class MarkedTicketsValidationHandler(
    ICheckMarkedTicketHandler checkMarkedTicketHandler,
    IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        var hasMarkedTickets = await checkMarkedTicketHandler.Handle(validationContext.Session.MatchId, validationContext.BetTypeRootXml);

        if (hasMarkedTickets)
        {
            return new Result
            {
                ErrCode = FastMarketErrorCodes.MarkedTicketsFound,
                ErrMsg = "Marked tickets found for the specified match and bet type"
            };
        }

        return new Result();
    }
}
