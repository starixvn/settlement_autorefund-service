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
    Task<(bool Success, List<Match> Matches)> Handle(int matchId, List<TargetResourceTrigger> triggerResources);
}

public class RefundConsumerHandler : IRefundConsumerHandler
{
    private readonly IGetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly IGetSrcResourcesHandler resourcesInfoHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly IGetRefundMatchInfoHandler getRefundMatchInfoHandler;
    private readonly IGetResultDetailsHandler getRefundResultsHandler;
    private readonly IGetAutoRefundResultsHandler getAutoRefundResultsHandler;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RefundConsumerHandler(
        IGetResourcesConditionsHandler getResourcesConditionsHandler,
        IGetSrcResourcesHandler resourcesInfoHandler,
        XmlConvertService xmlConvertService,
        IGetRefundMatchInfoHandler getRefundMatchInfoHandler,
        IGetResultDetailsHandler getRefundResultsHandler,
        IGetAutoRefundResultsHandler getAutoRefundResultsHandler)
    {
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
        this.resourcesInfoHandler = resourcesInfoHandler;
        this.xmlConvertService = xmlConvertService;
        this.getRefundMatchInfoHandler = getRefundMatchInfoHandler;
        this.getRefundResultsHandler = getRefundResultsHandler;
        this.getAutoRefundResultsHandler = getAutoRefundResultsHandler;
    }

    public async Task<(bool Success, List<Match> Matches)> Handle(int matchId, List<TargetResourceTrigger> triggerResources)
    {
        try
        {
            if (triggerResources.Count == 0)
            {
                return (false, new());
            }

            var relatedMatchItems = await GetRelatedMatchItems(matchId, triggerResources);
            Fanex.Logging.Logger.Log.Info($"GetRelatedMatchItems: {string.Join(",", relatedMatchItems.Select(x => x))}");

            if (relatedMatchItems.Count == 0)
            {
                return (false, new());
            }

            var validResults = await ProcessMatchResults(relatedMatchItems, triggerResources);
            if (validResults.Count == 0)
            {
                return (false, new());
            }

            var matches = relatedMatchItems.Select(x => BuildMatch(x.TargetMatchId, x.MatchType, validResults)).ToList();
            return (true, matches);
        }
        catch (Exception ex)
        {
            Fanex.Logging.Logger.Log.Error(ex.Message, ex);
            return (false, new());
        }
    }

    private async Task<List<RelatedMatchItem>> GetRelatedMatchItems(int matchId, List<TargetResourceTrigger> resources)
    {
        var requests = CreateRelatedMatchRequests(matchId, resources);
        var relatedMatchItems = await getRefundMatchInfoHandler.HandleAsync(requests);

        return [.. relatedMatchItems
            .DistinctBy(x => new
            {
                x.TargetMatchId,
                x.TargetChildMatchType,
            })];
    }

    private static List<GetRelatedMatchItemRequest> CreateRelatedMatchRequests(int matchId, List<TargetResourceTrigger> resources)
    {
        return [.. resources
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
            })];
    }

    private async Task<List<SourcesConditions>> ProcessMatchResults(
        List<RelatedMatchItem> relatedMatchItems,
        List<TargetResourceTrigger> triggerResources)
    {
        var matchIds = relatedMatchItems.Select(x => x.TargetMatchId).ToList();
        var matchTypes = relatedMatchItems.Select(x => x.MatchType).ToList();

        var validSourcesConditions = await getRefundResultsHandler.Handle(matchIds, triggerResources);
        var newValidSourcesConditions = await getAutoRefundResultsHandler.Handle(matchIds, validSourcesConditions);
        var conditions = await getResourcesConditionsHandler.GetConditionsAsync(matchTypes, newValidSourcesConditions);
        var sourceResources = await resourcesInfoHandler.HandleAsync(matchIds, conditions);

        return ExtractValidResults(sourceResources);
    }

    private static List<SourcesConditions> ExtractValidResults(List<SourcesConditions> sourceResources)
    {
        return [.. sourceResources
            .GroupBy(x => x.ResourceId)
            .SelectMany(group => ProcessResourceGroup(group))];
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
            OtherInfo = CreateOtherInfo(matchType, condition.RuleId, condition.Resource.IsConfirmedRefundSdt)
        };
    }

    private static string DetermineHomeValue(SourcesConditions condition)
    {
        return condition.ResourceId.StartsWith("381") || condition.ResourceId.StartsWith("382")
            ? "NT"
            : condition.Resource!.CfmHomeValue;
    }

    private static string CreateOtherInfo(short matchType, int ruleId, bool isRefundSdt)
    {
        return JsonSerializer.Serialize(
            new { matchType, ruleId, isRefundSdt },
            SerializerOptions);
    }

    private static SourcesConditions GetPreferredResource(IEnumerable<SourcesConditions> ruleGroup, string resourceKey)
        => ruleGroup.FirstOrDefault(x => x.SourceResourceId == resourceKey && x.Resource?.IsConfirmedRefundSdt == true)
        ?? ruleGroup.First();
}
