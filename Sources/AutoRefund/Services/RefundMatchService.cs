using AutoRefund.Core;
using AutoRefund.Core.Models;
using AutoRefund.Criteria;
using AutoRefund.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;

namespace AutoRefund.Services;

public interface IRefundMatchService
{
    Task<Result> Push(Match match);
}

public class RefundMatchServiceImp(
    IDynamicRepository repository,
    ScaleOutSettleService scaleOutSettleService,
    IOptions<AppSettingsKey> appSettingsKey,
    LogService logService,
    XmlConvertService xmlConvertService,
    IMatchService matchService)
    : IRefundMatchService
{
    public async Task<Result> Push(Match match)
    {
        var result = await ValidateBeforeExecute(match.MatchId);

        if (result.ErrCode == 0)
        {
            await scaleOutSettleService.UpdateUnderSettlementFlag(match.MatchId, 0, ActionName.Refund);
            await UpdateMatchInfo(match.MatchId);

            await WriteLog(match.MatchId);

            await Execute(match.MatchId);

            await matchService.UpdateStatus(match, ResultRefundStatus.Completed);
        }

        return result;
    }

    private async Task WriteLog(int matchId)
    {
        var insertLogCriteria = CreateLogCriteria(matchId);
        await logService.WriteLogResettle(insertLogCriteria);

        var message = GenerateLogMessage(matchId);
        await logService.WriteLog(matchId, message);
    }

    private static string GenerateLogMessage(int matchId)
        => $"NAP: Auto Calculate {ActionName.Refund} for matchId:{matchId} score FT: 0-0 HT: 0-0";

    private InsertLogCriteria CreateLogCriteria(int matchId)
    {
        const int soccer = 1;
        var resources = new List<ResourceItem>
        {
            new() { ResourceId = ResourceId.Halftime, HomeValue = string.Empty, AwayValue = string.Empty },
            new() { ResourceId = ResourceId.Final, HomeValue = string.Empty, AwayValue = string.Empty }
        };

        var selectedResources = xmlConvertService.Serialize(new Resources<ResourceItem>
        {
            ResourceItems = resources
        });

        return new InsertLogCriteria
        {
            MatchId = matchId,
            Action = ActionName.Refund,
            SportType = soccer,
            SelectedResources = selectedResources
        };
    }

    private async Task Execute(int matchId)
    {
        var criteria = new RefundPushCriteria
        {
            MatchId = matchId
        };

        var servers = appSettingsKey.Value.SiteDbServer;
        var tasks = servers.Select(server => ExecuteOneSiteDb(criteria, server)).ToList();
        await Task.WhenAll(tasks);
    }

    private async Task ExecuteOneSiteDb(RefundPushCriteria criteria, string dbServer)
    {
        criteria.DbServer = dbServer;
        await repository.ExecuteAsync(criteria);
    }

    private async Task<Result> ValidateBeforeExecute(int matchId)
    {
        const string mode = "ALL";
        var request = new EventTicketRequest
        {
            MatchId = matchId,
            Action = ActionName.Refund,
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

    private async Task UpdateMatchInfo(int matchId)
    {
        var requestUpdateInfo = new UpdateMatchInfoRequest
        {
            MatchId = matchId,
            Action = ActionName.Refund,
            AdminId = 0,
        };

        await scaleOutSettleService.UpdateMatchInfo(requestUpdateInfo);
    }
}