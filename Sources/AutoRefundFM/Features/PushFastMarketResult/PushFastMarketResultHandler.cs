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
    Task Handle(SpecialSession session, PushFastMarketResultCriteria criteria);
}

public class PushFastMarketResultHandler(
    ScaleOutSettleService scaleOutSettleService,
    IDynamicRepository repository,
    IOptions<AppSettingsKey> appSettingsKey,
    IUpdateWaterBreakHandler updateWaterBreakHandler,
    SettlementLogService logService,
    IDataTransformationService fastMarketDataTransformationService,
     IFmRefundResultService fmRefundResultService
        ) : IPushFastMarketResultHandler
{
    private readonly AppSettingsKey appSettingsKey = appSettingsKey.Value;

    public async Task Handle(SpecialSession session, PushFastMarketResultCriteria criteria)
    {
        var updateMatchInfoRequest = criteria.Adapt<UpdateMatchInfoRequest>();
        updateMatchInfoRequest.Mode = Mode.FastMarket;

        await scaleOutSettleService.UpdateMatchInfo(updateMatchInfoRequest);

        await WritePreSettlementLog(session.MatchId, criteria.Results);
        await WriteSettlementLog(session);

        await ExecuteCalculate(criteria);

        await updateWaterBreakHandler.Handle(session.MatchId, [session], 5);
        await fmRefundResultService.AddOrUpdateSessionResult(session.MatchId, [session], 2);

        await InsertLetstalkMessage(session);
    }

    private async Task WriteSettlementLog(SpecialSession session)
    {
        var (_, resourceXml) = fastMarketDataTransformationService.CreateResourcesFromFmResult(session.FastMarketResultDetails);

        var criteria = new InsertSettlementLogCriteria
        {
            MatchId = session.MatchId,
            Action = "REFUND",
            Mode = Mode.FastMarket,
            SportType = 1, //soccer
            IsAuto = true,
            SelectedResources = resourceXml,
        };

        await logService.WriteSettlementLog(criteria);
    }

    private async Task WritePreSettlementLog(int matchId, string betTypesXml)
    {
        var message = $"NAP: Fast Market REFUND for matchId: {matchId} with {betTypesXml}";

        await logService.WriteUserLogPreSettle(matchId, message);
    }

    private async Task ExecuteCalculate(PushFastMarketResultCriteria criteria)
    {
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            await repository.ExecuteAsync(criteria);
        }
    }

    private async Task InsertLetstalkMessage(SpecialSession session)
    {
        var date = DateTimeOffset.Now;
        var offsetHours = date.Offset.Hours;
        string executeTime = date.ToString("MMM dd, yyyy HH:mm:ss") + $" GMT{offsetHours:+#;-#;+0}";
        var message = $"The match has been executed Auto Refund WaterBreak {session} successfully at {executeTime}";
        var criteria = new AddNotificationCriteria
        {
            MatchId = session.MatchId,
            MessageContent = message,
            ActionName = $"Auto Refund WaterBreak {session}",
            MessageGroupId = 6, // Auto Refund FM
        };
        await repository.ExecuteAsync(criteria);
    }
}
