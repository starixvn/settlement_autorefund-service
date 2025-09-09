using AutoRefund.Core.Models;
using AutoRefundFM.Features.CheckMarkedTicket;
using AutoRefundFM.Models;

namespace AutoRefundFM.Services.Validation;

public class MarkedTicketsValidationHandler(
    ICheckMarkedTicketHandler checkMarkedTicketHandler,
    IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(SpecialSession session)
    {
        var betTypeRootXml = "";
        var hasMarkedTickets = await checkMarkedTicketHandler.Handle(session.MatchId, betTypeRootXml);

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
