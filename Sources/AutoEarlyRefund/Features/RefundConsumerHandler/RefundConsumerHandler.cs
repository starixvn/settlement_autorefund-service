using System.Text.Json;
using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Features.GetAutoRefundResults;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetRefundResults;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.RefundConsumerHandler;

public class RefundConsumerHandler
{
    readonly GetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly GetSrcResourcesHandler resourcesInfoHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly GetRefundMatchInfoHandler getRefundMatchInfoHandler;
    private readonly GetResultDetailsHandler getRefundResultsHandler;
    private readonly GetAutoRefundResultsHandler getAutoRefundResultsHandler;

    public RefundConsumerHandler(
        GetResourcesConditionsHandler getResourcesConditionsHandler,
        GetSrcResourcesHandler resourcesInfoHandler,
        XmlConvertService xmlConvertService,
        GetRefundMatchInfoHandler getRefundMatchInfoHandler,
        GetResultDetailsHandler getRefundResultsHandler,
        GetAutoRefundResultsHandler getAutoRefundResultsHandler)
    {
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
        this.resourcesInfoHandler = resourcesInfoHandler;
        this.xmlConvertService = xmlConvertService;
        this.getRefundMatchInfoHandler = getRefundMatchInfoHandler;
        this.getRefundResultsHandler = getRefundResultsHandler;
        this.getAutoRefundResultsHandler = getAutoRefundResultsHandler;
    }

    public async Task<(bool, Match)> Handle(int matchId, List<TargetResourceTrigger> triggerResources)
    {
        if (triggerResources.Count == 0) return (false, new Match());

        var relatedMatchItems = await GetRelatedMatchItems(matchId, triggerResources);

        if (relatedMatchItems.Count == 0) return (false, new Match());

        var matchType = relatedMatchItems[0].MatchType;
        var targetMatchId = relatedMatchItems[0].TargetMatchId;

        var validSourcesConditions = await getRefundResultsHandler.Handle(targetMatchId, triggerResources);
        var newValidSourcesConditions = await getAutoRefundResultsHandler.Handle(matchId, validSourcesConditions);

        var conditions = await getResourcesConditionsHandler.GetConditionsAsync(matchType, newValidSourcesConditions);
        var sourceResources = await resourcesInfoHandler.HandleAsync(targetMatchId, conditions);

        var validResults = ExtractValidResults(sourceResources);
        if (!validResults.Any()) return (false, new Match());

        var match = BuildMatch(targetMatchId, matchType, validResults);

        //return (false, new Match());
        return (true, match);
    }

    private async Task<List<RelatedMatchItem>> GetRelatedMatchItems(int matchId, List<TargetResourceTrigger> resources)
    {
        var getRelatedMatchItemRequest = resources.Select(x => new GetRelatedMatchItemRequest
        {
            MatchId = matchId,
            ChildMatchType = x.ChildMatchType,
            TargetChildMatchType = x.TargetResourceChildMatchType
        }).DistinctBy(x => new
        {
            x.ChildMatchType,
            x.TargetChildMatchType,
            x.MatchId
        }).ToList();

        var relatedMatchItems = await getRefundMatchInfoHandler.HandleAsync(getRelatedMatchItemRequest);

        return relatedMatchItems.DistinctBy(x => new
        {
            x.MatchType,
            x.TargetMatchId,
            x.TargetChildMatchType,
            x.MatchId,
        }).ToList();
    }

    private List<SourcesConditions> ExtractValidResults(List<SourcesConditions> sourceResources)
    {
        return sourceResources
            //.Where(x => x.Resource != null)
            .GroupBy(x => x.ResourceId)
            .SelectMany(group => ProcessResourceGroup(group))
            .ToList();
    }

    private IEnumerable<SourcesConditions> ProcessResourceGroup(IGrouping<string, SourcesConditions> resourceGroup)
    {
        foreach (var ruleGroup in resourceGroup.GroupBy(x => x.RuleId))
        {
            var specs = ruleGroup.Select(SpecificationFactory.CreateSpecification).ToList();
            var allValid = ruleGroup.All(sc => SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(sc));

            if (allValid)
            {
                yield return GetPreferredResource(ruleGroup, resourceGroup.Key);
                yield break;
            }
        }
    }

    private Match BuildMatch(int targetMatchId, short matchType, List<SourcesConditions> validResults)
    {
        return new Match
        {
            MatchId = targetMatchId,
            ResultDetails = validResults.Select(x => new ResultDetail
            {
                ResourceId = x.ResourceId,
                HomeValue = x.ResourceId.StartsWith("381") || x.ResourceId.StartsWith("382") ? "NT" : x.Resource!.CfmHomeValue,
                AwayValue = x.Resource!.CfmAwayValue,
                SourceId = x.RuleId.ToString(),
                OtherInfo = JsonSerializer.Serialize(new { matchType, ruleId = x.RuleId })
            }).ToList()
        }.WithResourcesXml(xmlConvertService);
    }

    private SourcesConditions GetPreferredResource(IEnumerable<SourcesConditions> ruleGroup, string resourceKey)
        => ruleGroup.FirstOrDefault(x => x.SourceResourceId == resourceKey && x.Resource?.IsConfirmedRefundSdt == true)
        ?? ruleGroup.First();
}
