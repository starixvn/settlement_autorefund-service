using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckConditionsRefund;

public class CheckConditionsRefundHandler(
    IValidateHandler next,
    IGetResourcesConditionsHandler conditionsHandler,
    IGetSrcResourcesHandler getResourcesInfoHandler,
    IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult,
    XmlConvertService xmlConvertService)
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

        var resources = await getResourcesInfoHandler.HandleAsync(match.MatchId, conditions);

        var validResults = GetValidResults(resources);


        match.ResultDetails = match.ResultDetails
            .Where(x => validResults.Exists(v => v.ResourceId == x.ResourceId))
            .ToList();

        await HandleInvalidResults(match, validResults);

        if (match.ResultDetails.Count == 0)
        {
            throw new AppException();
        }

        return next is not null && match.ResultDetails.Count != 0
            ? await next.HandleAsync(match)
            : new Result();
    }

    private static List<SourcesConditions> GetValidResults(List<SourcesConditions> resources)
        => resources
        .Where(x => x.Resource is not null)
        .GroupBy(x => x.ResourceId)
        .SelectMany(group =>
            group.GroupBy(x => x.RuleName)
                .Where(ruleGroup => ruleGroup.All(sc =>
                    SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(sc)))
                .Select(ruleGroup => ruleGroup.First())
                .Take(1))
        .ToList();


    private async Task HandleInvalidResults(Match match, List<SourcesConditions> validResults)
    {
        var invalidResults = match.ResultDetails
            .Where(x => validResults.TrueForAll(v => v.ResourceId != x.ResourceId))
            .ToList();

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
            ResultDetails = invalidResults.Select(x => new ResultDetail
            {
                ResourceId = x.ResourceId,
                HomeValue = x.HomeValue,
                AwayValue = x.AwayValue,
                Remark = "ERROR:This result did incorrect rule refund condition",
                SourceId = x.SourceId,
                OtherInfo = x.OtherInfo,
            }).ToList()
        }.WithResourcesXml(xmlConvertService);

        await insOrUpdAutoPayoutResult.Handle(failedMatch, AutoRefund.Core.ResultRefundStatus.Failed);
    }
}
