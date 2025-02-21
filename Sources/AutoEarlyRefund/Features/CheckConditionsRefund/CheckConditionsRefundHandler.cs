using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;

namespace AutoEarlyRefund.Features.CheckConditionsRefund;

public class CheckConditionsRefundHandler(
    IValidateHandler next,
    GetResourcesConditionsHandler conditionsHandler,
    GetResourcesInfoHandler getResourcesInfoHandler)
    : IValidateHandler
{
    public async Task<Result> HandleAsync(Match match)
    {
        var request = match.ResultDetails.Select(x => new SourcesConditionsItem
        {
            ResourceId = x.ResourceId,
            SportType = (short)x.SportType,
            ChildMatchType = (short)x.ChildMatchType,
            RuleId = x.Conditions!.RuleId,
            MatchType = x.Conditions!.MatchType,
        }).ToList();

        var conditions = await conditionsHandler.GetConditionsAsync(request);

        var resources = await getResourcesInfoHandler.HandleAsync(match.MatchId, conditions);

        var validResults = resources
            .Where(x => x.Resource is not null)
            .GroupBy(x => x.ResourceId)
            .SelectMany(group =>
                group.GroupBy(x => x.RuleName)
                    .Where(ruleGroup => ruleGroup.All(sc =>
                        SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(sc)))
                    .Select(ruleGroup => new
                    {
                        ResourceId = group.Key,
                        RuleName = ruleGroup.Key,
                    })
                    .Take(1)
            )
            .ToList();

        match.ResultDetails = match.ResultDetails
            .Where(x => validResults.Any(v => v.ResourceId == x.ResourceId))
            .ToList();

        var result = new Result
        {
            ErrCode = 0,
            ErrMsg = "SUCCESS"
        };


        if (result.IsSuccess && next is not null && match.ResultDetails.Any())
        {
            return await next.HandleAsync(match);
        }

        return result;
    }
}
