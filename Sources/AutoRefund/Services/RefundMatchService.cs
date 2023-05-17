using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;

namespace AutoRefund.Services;

public interface IRefundMatchService
{
    Task<Result> Push(Match match);
}

public class RefundMatchServiceImp : IRefundMatchService
{
    private readonly IDynamicRepository repository;

    private readonly AppSettingsKey appSettingsKey;

    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly LogDirectlyService logDirectlyService;

    public RefundMatchServiceImp(
        IDynamicRepository repository,
        ScaleOutSettleService scaleOutSettleService,
        IOptions<AppSettingsKey> appSettingsKey,
        LogDirectlyService logDirectlyService)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.repository = repository;
        this.scaleOutSettleService = scaleOutSettleService;
        this.logDirectlyService = logDirectlyService;
    }

    public async Task<Result> Push(Match match)
    {
        const string actionName = "REFUND";
        var result = await ValidateBeforeExecute(match, actionName);

        if (result.ErrCode == 0)
        {
            await scaleOutSettleService.UpdateUnderSettlementFlag(match.MatchId, 0, actionName);
            await UpdateMatchInfo(match, actionName);
            var criteria = new RefundPushCriteria
            {
                MatchId = match.MatchId
            };

            logDirectlyService.WriteLog(criteria.MatchId, GenerateLogMessage(criteria));

            Execute(criteria);
        }

        return result;
    }

    private string GenerateLogMessage(RefundPushCriteria criteria)
        => $"NAP: Auto Calculate {criteria.Action} for matchid:{criteria.MatchId} score FT: 0-0 HT: 0-0";

    private void Execute(RefundPushCriteria criteria)
    {
        foreach (var server in appSettingsKey.SiteDbServer)
        {
            criteria.DbServer = server;
            repository.Execute(criteria);
        }
    }

    private async Task<Result> ValidateBeforeExecute(Match match, string actionName)
    {
        const string mode = "ALL";
        var request = new EventTicketRequest
        {
            MatchId = match.MatchId,
            Action = actionName,
            SpName = "NAP_Admin_Settlement_Refund_1_Push",
            IsAuto = true,
            Mode = mode,
            IsAutoRefund = true
        };

        var resultMessage = await scaleOutSettleService.ValidateEventAndTicket(request);
        return new Result
        {
            ErrCode = resultMessage.ErrorCode,
            ErrMsg = resultMessage.ErrorMessage
        };
    }

    private async Task UpdateMatchInfo(Match match, string actionName)
    {
        var requestUpdateInfo = new UpdateMatchInfoRequest
        {
            MatchId = match.MatchId,
            Action = actionName,
            AdminId = 0,
        };

        await scaleOutSettleService.UpdateMatchInfo(requestUpdateInfo);
    }
}