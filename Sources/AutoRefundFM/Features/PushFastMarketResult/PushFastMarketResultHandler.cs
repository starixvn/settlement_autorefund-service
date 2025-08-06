using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Constants;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Mapster;
using Microsoft.Extensions.Options;


namespace AutoRefundFM.Features.PushFastMarketResult;

public interface IPushFastMarketResultHandler
{
    Task Handle(Match match, PushFastMarketResultCriteria criteria, string resourceXml);
}

public class PushFastMarketResultHandler(
    ScaleOutSettleService scaleOutSettleService,
    IDynamicRepository repository,
    IOptions<AppSettingsKey> appSettingsKey,
    IUpdateWaterBreakHandler updateWaterBreakHandler,
    SettlementLogService logService,
    IFmRefundResultService fmRefundResultService
        ) : IPushFastMarketResultHandler
{
    private readonly AppSettingsKey appSettingsKey = appSettingsKey.Value;

    public async Task Handle(Match match, PushFastMarketResultCriteria criteria, string resourceXml)
    {
        var updateMatchInfoRequest = criteria.Adapt<UpdateMatchInfoRequest>();
        updateMatchInfoRequest.Mode = Mode.FastMarket;

        await scaleOutSettleService.UpdateMatchInfo(updateMatchInfoRequest);

        await WritePreSettlementLog(match, criteria.Results);
        await WriteSettlementLog(match, resourceXml);

        await ExecuteCalculate(criteria);

        await updateWaterBreakHandler.Handle(match);
        await fmRefundResultService.AddOrUpdateSessionResult(match, 2);
        await fmRefundResultService.AddOrUpdateBetTypeResult(
            match.MatchId,
            [.. match.SpecialSessions.SelectMany(x => x.FastMarketResultDetails)],
            2,
            string.Empty);

    }

    private async Task WriteSettlementLog(Match match, string resourceXml)
    {
        var criteria = new InsertSettlementLogCriteria
        {
            MatchId = match.MatchId,
            Action = "REFUND",
            Mode = Mode.FastMarket,
            SportType = 1, //soccer
            IsAuto = true,
            SelectedResources = resourceXml,
        };

        await logService.WriteSettlementLog(criteria);
    }

    private async Task WritePreSettlementLog(Match match, string betTypesXml)
    {
        var message = $"NAP: Fast Market REFUND for matchId: {match.MatchId} with {betTypesXml}";

        await logService.WriteUserLogPreSettle(match.MatchId, message);
    }

    private async Task ExecuteCalculate(PushFastMarketResultCriteria criteria)
    {
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            await repository.ExecuteAsync(criteria);
        }
    }
}
