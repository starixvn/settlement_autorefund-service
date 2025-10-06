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
    IValidateHandler next = null) : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {
        var eventTicketRequest = CreateEventTicketRequest(match);
        var eventResult = await generalService.ExecuteSingle(eventTicketRequest.Adapt<ValidateEventCriteria>());
        if (eventResult.ErrorCode != 0)
        {
            return new Result
            {
                ErrCode = eventResult.ErrorCode,
                ErrMsg = eventResult.ErrorMessage
            };
        }

        var result = await RetryValidateRefundTicketAsync(eventTicketRequest, match);

        return result;
    }

    private EventTicketRequest CreateEventTicketRequest(Match match)
    {
        var matchWithResourcesXml = match.WithResourcesXml(xmlConvertService, false);
        return new EventTicketRequest
        {
            MatchId = matchWithResourcesXml.MatchId,
            Action = "REFUND",
            Mode = "EAR",
            SpName = "NAP_Admin_Settlement_Auto_1s_Push",
            IsAuto = true,
            SelectedResources = matchWithResourcesXml.ResourcesXml
        };
    }

    private async Task<Result> RetryValidateRefundTicketAsync(EventTicketRequest request, Match match)
    {
        var result = new Result();

        for (var attempt = 0; attempt < appSettingsKey.Value.RetryCount; attempt++)
        {
            var validateResults = await ValidateRefundTicketsOnAllServers(request);
            var firstError = GetFirstErrorResult(validateResults);

            if (firstError == null)
            {
                return await HandleSuccess(result, match);
            }

            if (ShouldRetry(firstError, attempt))
            {
                await Task.Delay(appSettingsKey.Value.RetryInterval);
                continue;
            }

            return await HandleError(result, firstError, match);
        }

        return await HandleSuccess(result, match);
    }

    private bool ShouldRetry(Result firstError, int attempt)
        => firstError?.ErrCode == 1_001_003 && attempt < appSettingsKey.Value.RetryCount - 1;

    private async Task<Result> HandleError(Result result, Result firstError, Match match)
    {
        result.ErrCode = firstError.ErrCode;
        result.ErrMsg = firstError.ErrMsg;
        return result.IsSuccess && next is not null ? await next.HandleAsync(match) : result;
    }

    private async Task<Result> HandleSuccess(Result result, Match match)
        => result.IsSuccess && next is not null ? await next.HandleAsync(match) : result;

    private async Task<List<Result>> ValidateRefundTicketsOnAllServers(EventTicketRequest request)
    {
        var tasks = appSettingsKey.Value.SiteDbServer
            .Select(server => CheckValidateRefundTicket(server, request.Adapt<ValidateRefundTicketCriteria>()));
        return (await Task.WhenAll(tasks)).ToList();
    }

    private static Result GetFirstErrorResult(List<Result> results)
    {
        return results.Find(x => x.ErrCode != 0);
    }

    private async Task<Result> CheckValidateRefundTicket(string dbServer, ValidateRefundTicketCriteria criteria)
    {
        criteria.DbServer = dbServer;
        await repository.ExecuteAsync(criteria);

        return new Result
        {
            ErrCode = criteria.ErrCode,
            ErrMsg = criteria.ErrMsg
        };
    }
}