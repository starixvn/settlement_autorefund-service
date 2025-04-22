using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Logging;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Features.CheckStatusRefundResults
{
    public class CheckStatusRefundResultsHandler(
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult,
        XmlConvertService xmlConvertService,
        ILogger<CheckStatusRefundResultsHandler> logger,
        IValidateHandler? next = null) : IValidateHandler
    {
        public async Task<Result> HandleAsync(Match match)
        {
            var settledResults = match.ResultDetails
                .Where(x => x.ResultStatusId == (int)ResultStatus.ManualSettled || x.ResultStatusId == (int)ResultStatus.AutoSettled);

            match.ResultDetails = [.. match.ResultDetails.Except(settledResults)];

            await HandleInvalidResults(match, [.. settledResults]);

            if (match.ResultDetails.Count == 0)
            {
                throw new AppException();
            }

            return next is not null ? await next.HandleAsync(match) : new Result();
        }

        private async Task HandleInvalidResults(Match match, List<ResultDetail> invalidResults)
        {
            Logger.Log.Info($"CheckStatusRefundResultsHandler: {match} {string.Join(",", invalidResults)}");
            logger.LogInformation($"CheckStatusRefundResultsHandler: {match} {string.Join(",", invalidResults)}");

            if (invalidResults.Count == 0)
            {
                return;
            }

            var failedMatch = new Match
            {
                MatchId = match.MatchId,
                LeagueId = match.LeagueId,
                SportType = match.SportType,
                ChildMatchType = match.ChildMatchType,
                ResultDetails = [.. invalidResults.Select(x => x.WithRemark("ERROR!: This result have been settled incorrectly already."))]
            }.WithResourcesXml(xmlConvertService);

            await insOrUpdAutoPayoutResult.Handle(failedMatch, AutoRefund.Core.ResultRefundStatus.Failed);
        }
    }
}
