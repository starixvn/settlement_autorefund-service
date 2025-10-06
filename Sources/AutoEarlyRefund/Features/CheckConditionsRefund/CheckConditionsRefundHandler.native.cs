using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetSrcResources;
using AutoEarlyRefund.Features.InsOrUpdAutoRefundResult;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using Fanex.Logging;

namespace AutoEarlyRefund.Features.CheckConditionsRefund;

public class CheckConditionsRefundHandler(
    IValidateHandler next,
    IGetResourcesConditionsHandler conditionsHandler,
    IGetSrcResourcesHandler getResourcesInfoHandler,
    IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult,
    XmlConvertService xmlConvertService,
    IWriteLogService writeLogService)
    : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {
        var request = match.ResultDetails.Select(x => new SourcesConditionsItem
        {
            ResourceId = x.ResourceId,
            SportType = x.SportType,
            ChildMatchType = x.ChildMatchType,
            RuleId = x.Conditions!.RuleId,
            MatchType = x.Conditions!.MatchType,
        }).ToList();

        var conditions = await conditionsHandler.GetConditionsAsync(request);

        var resources = await getResourcesInfoHandler.HandleAsync([match.MatchId], conditions);

        var validResults = GetValidResults(resources);
        var validResultsIds = validResults.Select(x => x.ResourceId).ToList();

        await HandleInvalidResults(match, validResultsIds);

        match.ResultDetails = [.. match.ResultDetails.Where(x => validResultsIds.Contains(x.ResourceId))];

        if (match.ResultDetails.Count == 0)
        {
            throw new AppException();
        }

        return next is not null && match.ResultDetails.Count != 0
            ? await next.HandleAsync(match)
            : new Result();
    }

    private static List<SourcesConditions> GetValidResults(List<SourcesConditions> resources)
        => [.. resources
        .Where(x => x.Resource is not null)
        .GroupBy(x => x.ResourceId)
        .SelectMany(group =>
            group.GroupBy(x => x.RuleName)
                .Where(ruleGroup => ruleGroup.All(sc =>
                    SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(sc)))
                .Select(ruleGroup => ruleGroup.First())
                .Take(1))];

    private async Task HandleInvalidResults(Match match, List<string> validResultsIds)
    {
        var invalidResults = match.ResultDetails
            .Where(x => validResultsIds.Count == 0 || !validResultsIds.Contains(x.ResourceId)).ToList();

        if (invalidResults.Count == 0)
        {
            return;
        }

        const string errorMessage = "ERROR:This result did incorrect rule refund condition";

        var failedMatch = new Match
        {
            MatchId = match.MatchId,
            LeagueId = match.LeagueId,
            SportType = match.SportType,
            ChildMatchType = match.ChildMatchType,
            ResultDetails = [.. invalidResults.Select(x => new ResultDetail
            {
                ResourceId = x.ResourceId,
                HomeValue = x.HomeValue,
                AwayValue = x.AwayValue,
                Remark = errorMessage,
                SourceId = x.SourceId,
                OtherInfo = x.OtherInfo,
            })]
        }.WithResourcesXml(xmlConvertService);

        await insOrUpdAutoPayoutResult.Handle(failedMatch, AutoRefund.Core.ResultRefundStatus.Failed);
        await writeLogService.WriteLogErrorSettle(failedMatch, errorMessage + ": " + string.Join(",", invalidResults.Select(x => x.ResourceId)));
    }
}