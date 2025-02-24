using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Settlement.ScaleOut.Criteria.Normal;
using Mapster;

namespace AutoEarlyRefund.Features.CheckEventAndTicket;

public class CheckEventAndTicketHandler(
    IValidateHandler next,
    GeneralService generalService,
    XmlConvertService xmlConvertService) : IValidateHandler
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

        if (eventResult.ErrorCode != 0)
        {
            return new Result
            {
                ErrCode = eventResult.ErrorCode,
                ErrMsg = eventResult.ErrorMessage
            };
        }

        var resultMsg = await generalService.ExecuteAsync(request.Adapt<ValidateRefundTicketCriteria>());

        var result = new Result
        {
            ErrCode = resultMsg.ErrorCode,
            ErrMsg = resultMsg.ErrorMessage
        };

        return result.IsSuccess && next is not null
            ? await next.HandleAsync(match)
            : result;
    }
}
