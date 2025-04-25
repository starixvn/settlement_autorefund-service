using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Logging;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Mapster;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Features.CheckEventAndTicket;

public class CheckEventAndTicketHandler(
    GeneralService generalService,
    XmlConvertService xmlConvertService,
    ILogger<CheckEventAndTicketHandler> logger,
    IOptions<AppSettingsKey> appSettingsKey,
    IValidateHandler? next = null) : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {
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
        Logger.Log.Info($"CheckEventHandler: {match} {eventResult.ErrorCode} {eventResult.ErrorMessage}");
        logger.LogInformation($"CheckEventHandler: {match} {eventResult.ErrorCode} {eventResult.ErrorMessage}");

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
            var resultMsg = await generalService.ExecuteAsync(request.Adapt<ValidateRefundTicketCriteria>());

            if (resultMsg.ErrorCode == 1_001_003 && i < appSettingsKey.Value.RetryCount - 1)
            {
                await Task.Delay(appSettingsKey.Value.RetryInterval);
                continue;
            }

            if (resultMsg.ErrorCode == 0)
            {
                break;
            }

            result.ErrCode = resultMsg.ErrorCode;
            result.ErrMsg = resultMsg.ErrorMessage;
        }

        Logger.Log.Info($"ValidateRefundTicketCriteria: {match} {result}");
        logger.LogInformation($"ValidateRefundTicketCriteria: {match} {result}");

        return result.IsSuccess && next is not null
            ? await next.HandleAsync(match)
            : result;
    }
}
