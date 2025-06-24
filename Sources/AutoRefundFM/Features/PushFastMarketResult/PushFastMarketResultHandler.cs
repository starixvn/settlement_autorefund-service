using Fanex.Settlement.ScaleOut.Constants;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;

namespace AutoRefundFM.Features.PushFastMarketResult;

public class PushFastMarketResultHandler
{
    private readonly ScaleOutSettleService scaleOutSettleService;

    public PushFastMarketResultHandler(ScaleOutSettleService scaleOutSettleService)
    {
        this.scaleOutSettleService = scaleOutSettleService;
    }

    public async Task Handle()
    {
        var criteria = new PushFastMarketResultCriteria();
        var validRequest = new EventTicketFastMarketRequest
        {

        };
        var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        if (validationResult.ErrorCode > 0)
        {
            return;
        }


        var updateMatchInfoRequest = new UpdateMatchInfoRequest
        {
            MatchId = criteria.MatchId,
            Action = criteria.Action,
            AdminId = criteria.AdminId,
            Results = criteria.Results,
            Mode = Mode.FastMarket,
            IsSettleDetermined = criteria.IsSettleDetermined,
            IsAuto = criteria.IsAuto
        };

        await scaleOutSettleService.UpdateMatchInfo(updateMatchInfoRequest);

        //log
        //excute
        // push result amend

    }
}
