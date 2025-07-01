using AutoRefund.Core.Models;
using AutoRefund.Criteria;
using AutoRefund.Services;
using AutoRefundFM.Features.GetFastMarketResults;
using AutoRefundFM.Features.InsOrUpdAutoRefundResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut.Constants;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;


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
    private readonly LogService logService;
    private readonly IDynamicRepository repository;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler;
    private readonly IUpdateWaterBreakHandler updateWaterBreakHandler;
    private readonly AppSettingsKey appSettingsKey;

    public PushFastMarketResultHandler(
        ScaleOutSettleService scaleOutSettleService,
        IGetFastMarketResultsHandler getFastMarketResultsHandler,
        XmlConvertService xmlConvertService,
        LogService logService,
        IDynamicRepository repository,
        IOptions<AppSettingsKey> appSettingsKey,
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
        IUpdateWaterBreakHandler updateWaterBreakHandler
        )
    {
        this.scaleOutSettleService = scaleOutSettleService;
        this.getFastMarketResultsHandler = getFastMarketResultsHandler;
        this.xmlConvertService = xmlConvertService;
        this.logService = logService;
        this.repository = repository;
        this.appSettingsKey = appSettingsKey.Value;
        this.insOrUpdAutoPayoutResultHandler = insOrUpdAutoPayoutResultHandler;
        this.updateWaterBreakHandler = updateWaterBreakHandler;
    }

    public async Task Handle(Match match)
    {
        var betTypes = await getFastMarketResultsHandler.Handle(match);
        var betTypesXml = xmlConvertService.Serialize(betTypes);
        var resourceXml = CreateResourceItemsFromBetTypes(betTypes);

        //var criteria = new PushFastMarketResultCriteria
        //{
        //    MatchId = match.MatchId,
        //    Results = betTypesXml,
        //};
        //var validRequest = criteria.Adapt<EventTicketFastMarketRequest>();
        //validRequest.IsAuto = false;
        //var validationResult = await scaleOutSettleService.ValidateEventAndTicketFastMarket(validRequest);

        //if (validationResult.ErrorCode > 0)
        //{
        //    return;
        //}

        //var updateMatchInfoRequest = criteria.Adapt<UpdateMatchInfoRequest>();
        //updateMatchInfoRequest.Mode = Mode.FastMarket;

        //await scaleOutSettleService.UpdateMatchInfo(updateMatchInfoRequest);

        //await WritePreSettlementLog(match, betTypesXml);
        //await WriteSettlementLog(match, resourceXml);

        //ExecuteCalculate(criteria);

        await updateWaterBreakHandler.Handle(match);
        await insOrUpdAutoPayoutResultHandler.Handle(match.MatchId, 2, resourceXml);
    }

    private async Task WriteSettlementLog(Match match, string resourceXml)
    {
        var criteria = new InsertLogCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            Mode = Mode.FastMarket,
            SportType = 1, //soccer
            IsAuto = true,
            SelectedResources = resourceXml,
        };

        await logService.WriteLogResettle(criteria);
    }

    private async Task WritePreSettlementLog(Match match, string betTypesXml)
    {
        var message = $"NAP: Fast Market REFUND for matchId: {match.MatchId} with {betTypesXml}";

        await logService.WriteLog(match.MatchId, message);
    }

    private void ExecuteCalculate(PushFastMarketResultCriteria criteria)
    {
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            repository.Execute(criteria);
        }
    }
    private string CreateResourceItemsFromBetTypes(BetTypesRoot betTypesRoot)
    {
        var resourceItems = betTypesRoot.BetType
              .SelectMany(betType => betType.Items.Select(item => new ResourceItem
              {
                  ResourceId = betType.Id.ToString(),
                  OtherInfo = item.Position.ToString()
              }))
              .ToList();

        var resourceRoot = new Resources<ResourceItem>
        {
            ResourceItems = resourceItems
        };

        var selectedResources = xmlConvertService.Serialize(resourceRoot);

        return selectedResources;
    }
}
