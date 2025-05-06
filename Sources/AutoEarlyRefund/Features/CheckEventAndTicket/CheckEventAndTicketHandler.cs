using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Mapster;

namespace AutoEarlyRefund.Features.CheckEventAndTicket;

public class CheckEventAndTicketHandler(
    GeneralService generalService,
    IDynamicRepository repository,
    XmlConvertService xmlConvertService,
    IOptions<AppSettingsKey> appSettingsKey,
    IValidateHandler? next = null) : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {
        await Fanex.Logging.Logger.Log.WarnAsync($"Before CheckEventAndTicketHandler: {match.MatchId} {string.Join(",", match.ResultDetails.Select(x => x.ResourceId))} at {DateTime.Now}");
        var newMatch = match.WithResourcesXml(xmlConvertService, false);
        var request = new EventTicketRequest
        {
            MatchId = newMatch.MatchId,
            Action = "REFUND",
            Mode = "EAR",
            SpName = "NAP_Admin_Settlement_Auto_1s_Push",
            IsAuto = true,
            SelectedResources = newMatch.ResourcesXml
        };

        var eventResult = await generalService.ExecuteSingle(request.Adapt<ValidateEventCriteria>());

        if (eventResult.ErrorCode != 0)
        {
            return new Result
            {
                ErrCode = eventResult.ErrorCode,
                ErrMsg = eventResult.ErrorMessage
            };
        }

        var result = new Result();
        for (var i = 0; i < appSettingsKey.Value.RetryCount; i++)
        {
            var tasks = appSettingsKey.Value.SiteDbServer.Select(s => CheckValidateRefundTicket(s, request.Adapt<ValidateRefundTicketCriteria>()));
            var results = await Task.WhenAll(tasks.ToArray());

            var inValidResult = results.FirstOrDefault(x => x.ErrCode != 0);

            if (inValidResult == null)
            {
                break;
            }

            if (inValidResult.ErrCode == 1_001_003 && i < appSettingsKey.Value.RetryCount - 1)
            {
                await Fanex.Logging.Logger.Log.WarnAsync("After CheckEventAndTicketHandler ErrorCode InQueue");
                await Task.Delay(appSettingsKey.Value.RetryInterval);
                continue;
            }

            result.ErrCode = inValidResult.ErrCode;
            result.ErrMsg = inValidResult.ErrMsg;

            break;
        }

        await Fanex.Logging.Logger.Log.WarnAsync($"After CheckEventAndTicketHandler: {match.MatchId} {string.Join(",", match.ResultDetails.Select(x => x.ResourceId))} at {DateTime.Now}");

        return result.IsSuccess && next is not null
            ? await next.HandleAsync(match)
            : result;
    }

    private async Task<Result> CheckValidateRefundTicket(string s, ValidateRefundTicketCriteria criteria)
    {
        criteria.DbServer = s;
        await repository.ExecuteAsync(criteria);

        return new Result
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg
        };
    }
}
