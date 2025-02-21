using System.Text.Json;
using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer : IConsumer<ResultsRefunded>, IConsumer<ResultsRefundConfirmedSdt>,
    IConsumer<ResultsRefundConfirmed>, IConsumer<MatchCalculated>, IConsumer<MainMatchCalculated>,
    IConsumer<ExtraMatchCalculated>, IConsumer<ResultsCalculated>
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly GetResourcesTriggerHandler getResourcesTriggerHandler;
    readonly GetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly GetResourcesInfoHandler resourcesInfoHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly GetRefundMatchInfoHandler getRefundMatchInfoHandler;

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        GetResourcesTriggerHandler getResourcesTriggerHandler,
        GetResourcesConditionsHandler getResourcesConditionsHandler,
        GetResourcesInfoHandler resourcesInfoHandler,
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
        XmlConvertService xmlConvertService,
        GetRefundMatchInfoHandler getRefundMatchInfoHandler)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
        this.resourcesInfoHandler = resourcesInfoHandler;
        this.insOrUpdAutoPayoutResultHandler = insOrUpdAutoPayoutResultHandler;
        this.xmlConvertService = xmlConvertService;
        this.getRefundMatchInfoHandler = getRefundMatchInfoHandler;
    }

    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        logger.LogInformation($"ResultsRefunded Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        await HandleConsumer(context.Message.MatchId, triggerResources);

        logger.LogInformation($"ResultsRefunded Done: {context.Message}");
    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmedSdt> context)
    {
        logger.LogInformation($"ResultsRefundConfirmedSdt Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        await HandleConsumer(context.Message.MatchId, triggerResources);

        logger.LogInformation($"ResultsRefundConfirmedSdt Done: {context.Message}");

    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmed> context)
    {
        logger.LogInformation($"ResultsRefundConfirmed Received: {context.Message}");
        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        await HandleConsumer(context.Message.MatchId, triggerResources);

        logger.LogInformation($"ResultsRefundConfirmed Done: {context.Message}");

    }

    public async Task Consume(ConsumeContext<MatchCalculated> context)
    {
        logger.LogInformation($"MatchCalculated Received: {context.Message}");
        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            "00");

        await HandleConsumer(context.Message.MatchId, triggerResources);

        logger.LogInformation($"MatchCalculated Done: {context.Message}");

    }


    public async Task Consume(ConsumeContext<MainMatchCalculated> context)
    {
        logger.LogInformation($"MatchCalculated Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            "00");

        await HandleConsumer(context.Message.MatchId, triggerResources);

        logger.LogInformation($"MatchCalculated Done: {context.Message}");

    }

    public async Task Consume(ConsumeContext<ExtraMatchCalculated> context)
    {
        logger.LogInformation($"ExtraMatchCalculated Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            "00");

        await HandleConsumer(context.Message.MatchId, triggerResources);

        logger.LogInformation($"ExtraMatchCalculated Done: {context.Message}");
    }

    private async Task HandleConsumer(int matchId, List<TargetResourceTrigger> triggerResources)
    {
        if (triggerResources.Count == 0)
        {
            return;
        }

        var relatedMatchItems = await GetRelatedMatchItems(matchId, triggerResources);

        if (relatedMatchItems.Count == 0)
        {
            return;
        }

        var matchType = relatedMatchItems.First().MatchType;
        var targetMatchId = relatedMatchItems.First().TargetMatchId;

        var conditions = await getResourcesConditionsHandler.GetConditionsAsync(matchType, triggerResources);
        var sourceResources = await resourcesInfoHandler.HandleAsync(targetMatchId, conditions);

        var newSourceResources = sourceResources
            .Where(x => x.Resource is not null)
            .GroupBy(x => x.ResourceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var validResults = new List<SourcesConditions>();
        foreach (var (resourceId, item) in newSourceResources)
        {
            var ruleGroup = item
                .GroupBy(x => x.RuleId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (ruleId, condition) in ruleGroup)
            {
                var valid = condition.All(sc =>
                {
                    var spec = SpecificationFactory.CreateSpecification(sc);
                    var isSatisfiedBy = spec.IsSatisfiedBy(sc);
                    return isSatisfiedBy;
                });

                if (valid)
                {
                    var validCondition = condition.FirstOrDefault(x => x.SourceResourceId == resourceId && x.Resource.IsConfirmedRefundSdt)
                        ?? condition.First();

                    validResults.Add(validCondition);
                    break;
                }

            }
        }

        if (validResults.Count == 0)
        {
            return;
        }

        var match = BuildMatch(targetMatchId, matchType, validResults);

        await this.insOrUpdAutoPayoutResultHandler.Handle(match, AutoRefund.Core.ResultRefundStatus.Initial);
    }



    private Match BuildMatch(int targetMatchId, short matchType, List<SourcesConditions> validResults)
    {
        return new Match
        {
            MatchId = targetMatchId,
            ResultDetails = validResults.Select(x => new ResultDetail
            {
                ResourceId = x.ResourceId,
                HomeValue = x.Resource.CfmHomeValue,
                AwayValue = x.Resource.CfmAwayValue,
                SourceId = x.RuleId.ToString(),
                OtherInfo = JsonSerializer.Serialize(new { matchType, ruleId = x.RuleId })
            }).ToList()
        }.WithResourcesXml(xmlConvertService);
    }

    private async Task<List<RelatedMatchItem>> GetRelatedMatchItems(int matchId, List<TargetResourceTrigger> resources)
    {
        var getRelatedMatchItemRequest = resources.Select(x => new GetRelatedMatchItemRequest
        {
            MatchId = matchId,
            ChildMatchType = x.ChildMatchType,
            TargetChildMatchType = x.TargetResourceChildMatchType
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

    public async Task Consume(ConsumeContext<ResultsCalculated> context)
    {
        logger.LogInformation($"ResultsRefundConfirmedSdt Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        await HandleConsumer(context.Message.MatchId, triggerResources);

        logger.LogInformation($"ResultsRefundConfirmedSdt Done: {context.Message}");

    }
}