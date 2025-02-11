using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer : IConsumer<ResultsRefunded>, IConsumer<ResultsRefundConfirmed>
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly GetResourcesTriggerHandler getResourcesTriggerHandler;
    readonly GetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly GetResourcesInfoHandler resourcesInfoHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler;
    private readonly XmlConvertService xmlConvertService;

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        GetResourcesTriggerHandler getResourcesTriggerHandler,
        GetResourcesConditionsHandler getResourcesConditionsHandler,
        GetResourcesInfoHandler resourcesInfoHandler,
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
        XmlConvertService xmlConvertService)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
        this.resourcesInfoHandler = resourcesInfoHandler;
        this.insOrUpdAutoPayoutResultHandler = insOrUpdAutoPayoutResultHandler;
        this.xmlConvertService = xmlConvertService;
    }

    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        logger.LogInformation("Refund result received: {MatchId}", context.Message.MatchId);

        var criteria = new GetResourcesTriggerCriteria
        {
            Action = context.Message.EventId,
            SportType = 1,
            ChildMatchType = 202,
            ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
        };

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(criteria);
        if (triggerResources.Count == 0)
        {
            logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
            return;
        }
        var resourceConditions = await getResourcesConditionsHandler.HandleAsync(triggerResources);
        var sourceResources = await resourcesInfoHandler.HandleAsync(context.Message.MatchId, resourceConditions);

        var validResults = sourceResources
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

        var match = new Match
        {
            MatchId = context.Message.MatchId,
            ResultDetails = validResults.Select(x => new ResultDetail
            {
                ResourceId = x.ResourceId,
                HomeValue = "",
                AwayValue = "",
                Remark = x.RuleName,
            }).ToList()
        }.WithResourcesXml(xmlConvertService);


        //await this.insOrUpdAutoPayoutResultHandler.Handle(match, 0);

        logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmed> context)
    {
        logger.LogInformation("Refund result received: {MatchId}", context.Message.MatchId);
        var criteria = new GetResourcesTriggerCriteria
        {
            Action = context.Message.EventId,
            SportType = 1,
            ChildMatchType = 202,
            ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
        };

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(criteria);
        if (triggerResources.Count == 0)
        {
            logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);
            return;
        }
        var conditions = await getResourcesConditionsHandler.HandleAsync(triggerResources);
        var sourceResources = await resourcesInfoHandler.HandleAsync(context.Message.MatchId, conditions);

        var z = sourceResources
            .GroupBy(x => new { x.ResourceId, x.RuleName })
            .ToDictionary(g => g.Key, g => g.ToList());

        var validResults = new List<string>();
        foreach (var (ruleKey, sourceCondition) in z)
        {
            var valid = sourceCondition.All(sc =>
            {
                var spec = SpecificationFactory.CreateSpecification(sc);
                var isSatisfiedBy = spec.IsSatisfiedBy(sc);
                return isSatisfiedBy;
            });

            if (valid)
            {
                validResults.Add(ruleKey.ResourceId);
                break;
            }
        }

        logger.LogInformation("Refund result Done: {MatchId}", context.Message.MatchId);

    }
}