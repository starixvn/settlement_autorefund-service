using AutoEarlyRefund.Features.InsOrUpdAutoRefundResult;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Logging;

namespace AutoEarlyRefund.Features.CheckStatusRefundResults
{
    public class CheckStatusRefundResultsHandler(
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult,
        XmlConvertService xmlConvertService,
        IWriteLogService writeLogService,
        IValidateHandler next = null) : IValidateHandler
    {
        public async Task<Result> HandleAsync(Match match)
        {
            await LogService.InfoAsync($"Before CheckStatusRefundResultsHandler: {match.MatchId} {string.Join(",", match.ResultDetails.Select(x => x.ResourceId))} at {DateTimeOffset.Now}");

            var settledResults = match.ResultDetails
                .Where(x => x.ResultStatusId is (int)ResultStatus.ManualSettled or (int)ResultStatus.AutoSettled);

            match.ResultDetails = [.. match.ResultDetails.Except(settledResults)];

            await HandleInvalidResults(match, [.. settledResults]);

            await LogService.InfoAsync($"After CheckStatusRefundResultsHandler: {match.MatchId} {string.Join(",", match.ResultDetails.Select(x => x.ResourceId))} at {DateTimeOffset.Now}");

            if (match.ResultDetails.Count == 0)
            {
                throw new AppException();
            }

            return next is not null ? await next.HandleAsync(match) : new Result();
        }

        private async Task HandleInvalidResults(Match match, List<ResultDetail> invalidResults)
        {
            const string errorMessage = "ERROR: This result have been settled incorrectly already";

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
                ResultDetails = [.. invalidResults.Select(x => x.WithRemark(errorMessage))]
            }.WithResourcesXml(xmlConvertService);

            await insOrUpdAutoPayoutResult.Handle(failedMatch, AutoRefund.Core.ResultRefundStatus.Failed);

            await writeLogService.WriteLogErrorSettle(failedMatch, errorMessage + ": " + string.Join(", ", invalidResults.Select(x => x.ResourceId)));
        }
    }
}