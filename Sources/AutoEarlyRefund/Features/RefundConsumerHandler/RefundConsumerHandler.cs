using System.Text.Json;
using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Features.GetAutoRefundResults;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetRefundResults;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.RefundConsumerHandler;

public interface IRefundConsumerHandler
{
    Task<(bool Success, Match Match)> Handle(int matchId, List<TargetResourceTrigger> triggerResources);
}

public class RefundConsumerHandler : IRefundConsumerHandler
{
    private readonly GetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly GetSrcResourcesHandler resourcesInfoHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly GetRefundMatchInfoHandler getRefundMatchInfoHandler;
    private readonly GetResultDetailsHandler getRefundResultsHandler;
    private readonly GetAutoRefundResultsHandler getAutoRefundResultsHandler;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RefundConsumerHandler(
        GetResourcesConditionsHandler getResourcesConditionsHandler,
        GetSrcResourcesHandler resourcesInfoHandler,
        XmlConvertService xmlConvertService,
        GetRefundMatchInfoHandler getRefundMatchInfoHandler,
        GetResultDetailsHandler getRefundResultsHandler,
        GetAutoRefundResultsHandler getAutoRefundResultsHandler)
    {
        this.getResourcesConditionsHandler = getResourcesConditionsHandler ?? throw new ArgumentNullException(nameof(getResourcesConditionsHandler));
        this.resourcesInfoHandler = resourcesInfoHandler ?? throw new ArgumentNullException(nameof(resourcesInfoHandler));
        this.xmlConvertService = xmlConvertService ?? throw new ArgumentNullException(nameof(xmlConvertService));
        this.getRefundMatchInfoHandler = getRefundMatchInfoHandler ?? throw new ArgumentNullException(nameof(getRefundMatchInfoHandler));
        this.getRefundResultsHandler = getRefundResultsHandler ?? throw new ArgumentNullException(nameof(getRefundResultsHandler));
        this.getAutoRefundResultsHandler = getAutoRefundResultsHandler ?? throw new ArgumentNullException(nameof(getAutoRefundResultsHandler));
    }

    public async Task<(bool Success, Match Match)> Handle(int matchId, List<TargetResourceTrigger> triggerResources)
    {
        ArgumentNullException.ThrowIfNull(triggerResources);
        if (matchId <= 0) throw new ArgumentException("Match ID must be greater than 0", nameof(matchId));

        if (!triggerResources.Any())
        {
            return (false, CreateEmptyMatch());
        }

        var relatedMatchItems = await GetRelatedMatchItems(matchId, triggerResources);
        if (!relatedMatchItems.Any())
        {
            return (false, CreateEmptyMatch());
        }

        var matchInfo = relatedMatchItems[0];
        var validResults = await ProcessMatchResults(matchId, matchInfo, triggerResources);
        if (!validResults.Any())
        {
            return (false, CreateEmptyMatch());
        }

        var match = BuildMatch(matchInfo.TargetMatchId, matchInfo.MatchType, validResults);
        return (true, match);
    }

    private async Task<List<RelatedMatchItem>> GetRelatedMatchItems(int matchId, List<TargetResourceTrigger> resources)
    {
        var requests = CreateRelatedMatchRequests(matchId, resources);
        var relatedMatchItems = await getRefundMatchInfoHandler.HandleAsync(requests);

        return relatedMatchItems
            .DistinctBy(x => new
            {
                x.MatchType,
                x.TargetMatchId,
                x.TargetChildMatchType,
                x.MatchId,
            })
            .ToList();
    }

    private static List<GetRelatedMatchItemRequest> CreateRelatedMatchRequests(int matchId, List<TargetResourceTrigger> resources)
    {
        return resources
            .Select(x => new GetRelatedMatchItemRequest
            {
                MatchId = matchId,
                ChildMatchType = x.ChildMatchType,
                TargetChildMatchType = x.TargetResourceChildMatchType
            })
            .DistinctBy(x => new
            {
                x.ChildMatchType,
                x.TargetChildMatchType,
                x.MatchId
            })
            .ToList();
    }

    private async Task<List<SourcesConditions>> ProcessMatchResults(
        int matchId,
        RelatedMatchItem matchInfo,
        List<TargetResourceTrigger> triggerResources)
    {
        var validSourcesConditions = await getRefundResultsHandler.Handle(matchInfo.TargetMatchId, triggerResources);
        var newValidSourcesConditions = await getAutoRefundResultsHandler.Handle(matchId, validSourcesConditions);
        var conditions = await getResourcesConditionsHandler.GetConditionsAsync(matchInfo.MatchType, newValidSourcesConditions);
        var sourceResources = await resourcesInfoHandler.HandleAsync(matchInfo.TargetMatchId, conditions);

        return ExtractValidResults(sourceResources);
    }

    private List<SourcesConditions> ExtractValidResults(List<SourcesConditions> sourceResources)
    {
        return sourceResources
            .GroupBy(x => x.ResourceId)
            .SelectMany(group => ProcessResourceGroup(group))
            .ToList();
    }

    private static IEnumerable<SourcesConditions> ProcessResourceGroup(IGrouping<string, SourcesConditions> resourceGroup)
    {
        foreach (var ruleGroup in resourceGroup.GroupBy(x => x.RuleId))
        {
            if (AreAllSpecificationsSatisfied(ruleGroup))
            {
                yield return GetPreferredResource(ruleGroup, resourceGroup.Key);
                yield break;
            }
        }
    }

    private static bool AreAllSpecificationsSatisfied(IEnumerable<SourcesConditions> ruleGroup)
    {
        return ruleGroup.All(sc => SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(sc));
    }

    private Match BuildMatch(int targetMatchId, short matchType, List<SourcesConditions> validResults)
    {
        return new Match
        {
            MatchId = targetMatchId,
            ResultDetails = validResults.Select(x => CreateResultDetail(x, matchType)).ToList()
        }.WithResourcesXml(xmlConvertService);
    }

    private static ResultDetail CreateResultDetail(SourcesConditions condition, short matchType)
    {
        return new ResultDetail
        {
            ResourceId = condition.ResourceId,
            HomeValue = DetermineHomeValue(condition),
            AwayValue = condition.Resource!.CfmAwayValue,
            SourceId = condition.RuleId.ToString(),
            OtherInfo = CreateOtherInfo(matchType, condition.RuleId)
        };
    }

    private static string DetermineHomeValue(SourcesConditions condition)
    {
        return condition.ResourceId.StartsWith("381") || condition.ResourceId.StartsWith("382")
            ? "NT"
            : condition.Resource!.CfmHomeValue;
    }

    private static string CreateOtherInfo(short matchType, int ruleId)
    {
        return JsonSerializer.Serialize(
            new { matchType, ruleId },
            SerializerOptions);
    }

    private static SourcesConditions GetPreferredResource(IEnumerable<SourcesConditions> ruleGroup, string resourceKey)
        => ruleGroup.FirstOrDefault(x => x.SourceResourceId == resourceKey && x.Resource?.IsConfirmedRefundSdt == true)
        ?? ruleGroup.First();

    private static Match CreateEmptyMatch() => new();
}
