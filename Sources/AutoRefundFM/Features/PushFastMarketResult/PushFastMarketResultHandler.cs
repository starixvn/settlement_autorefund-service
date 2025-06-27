using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Models;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Constants;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;

namespace AutoRefundFM.Features.PushFastMarketResult;

public interface IPushFastMarketResultHandler
{
    Task Handle(Match match);
}

public class PushFastMarketResultHandler : IPushFastMarketResultHandler
{
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly IGetFastMarketResultsHandler getFastMarketResultsHandler;
    private readonly XmlConvertService xmlConvertService;

    public PushFastMarketResultHandler(ScaleOutSettleService scaleOutSettleService, IGetFastMarketResultsHandler getFastMarketResultsHandler, XmlConvertService xmlConvertService)
    {
        this.scaleOutSettleService = scaleOutSettleService;
        this.getFastMarketResultsHandler = getFastMarketResultsHandler;
        this.xmlConvertService = xmlConvertService;
    }

    public async Task Handle(Match match)
    {
        var betTypes = await getFastMarketResultsHandler.Handle(match);
        var betTypesXml = xmlConvertService.Serialize(betTypes);

        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = match.MatchId,
            Results = betTypesXml,
        };
        var validRequest = criteria.Adapt<EventTicketFastMarketRequest>();
        var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        if (validationResult.ErrorCode > 0)
        {
            return;
        }

        var updateMatchInfoRequest = criteria.Adapt<UpdateMatchInfoRequest>();
        updateMatchInfoRequest.Mode = Mode.FastMarket;


        await scaleOutSettleService.UpdateMatchInfo(updateMatchInfoRequest);

        //log
        //excute
        // push result amend

    }
}
