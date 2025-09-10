using AutoRefund.Core.Models;
using AutoRefundFM.Features.PushFastMarketResult;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;

namespace AutoRefundFM.Services.Validation;

public class EventTicketValidationHandler(
    ScaleOutSettleService scaleOutSettleService,
    IValidationHandler? next = null) : ValidationHandlerBase(next)
{
    protected override async Task<Result> ProcessAsync(ValidationContext validationContext)
    {
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = validationContext.Session.MatchId,
            Results = validationContext.BetTypeRootXml,
            EventCounters = validationContext.EventCountersXml
        };

        var validRequest = criteria.Adapt<EventTicketFastMarketRequest>();
        validRequest.IsAuto = false;

        var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        return new Result
        {
            ErrCode = validationResult.ErrorCode,
            ErrMsg = validationResult.ErrorMessage
        };
    }
}
