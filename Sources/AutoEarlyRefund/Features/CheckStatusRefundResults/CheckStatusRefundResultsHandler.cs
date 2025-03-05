using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckStatusRefundResults
{
    public class CheckStatusRefundResultsHandler : IValidateHandler
    {
        private readonly IValidateHandler? next;
        private readonly XmlConvertService xmlConvertService;
        private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult;

        public CheckStatusRefundResultsHandler(
            IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult,
            XmlConvertService xmlConvertService,
            IValidateHandler? next = null)
        {
            this.xmlConvertService = xmlConvertService;
            this.next = next;
            this.insOrUpdAutoPayoutResult = insOrUpdAutoPayoutResult;
        }

        public async Task<Result> HandleAsync(Match match)
        {
            var settledResults = match.ResultDetails.Where(x => x.ResultStatusId == (int)SettleStatus.Calculated || x.ResultStatusId == (int)ResultStatus.AutoSettled);

            match.ResultDetails = match.ResultDetails.Except(settledResults).ToList();

            await HandleInvalidResults(match, settledResults.ToList());

            if (match.ResultDetails.Count == 0)
            {
                throw new AppException();
            }

            return next is not null ? await next.HandleAsync(match) : new Result();
        }

        private async Task HandleInvalidResults(Match match, List<ResultDetail> invalidResults)
        {
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
                ResultDetails = invalidResults.Select(x => x.WithRemark("ERROR!: This result have been settled incorrectly already.")).ToList()
            }.WithResourcesXml(xmlConvertService);

            await insOrUpdAutoPayoutResult.Handle(failedMatch, AutoRefund.Core.ResultRefundStatus.Failed);
        }
    }
}
