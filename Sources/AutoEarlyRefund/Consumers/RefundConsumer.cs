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

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        GetResourcesTriggerHandler getResourcesTriggerHandler,
        GetResourcesConditionsHandler getResourcesConditionsHandler,
        GetResourcesInfoHandler resourcesInfoHandler,
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
        this.resourcesInfoHandler = resourcesInfoHandler;
        this.insOrUpdAutoPayoutResultHandler = insOrUpdAutoPayoutResultHandler;
    }

    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        logger.LogInformation("Refund result received: {MatchId}", context.Message.MatchId);

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(new GetResourcesTriggerCriteria
        {
            Action = context.Message.Action,
            SportType = context.Message.ShortType,
            ChildMatchType = context.Message.ChildMatchType,
            ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
        });

        var resourceConditions = await getResourcesConditionsHandler.HandleAsync(triggerResources);

        var resultDetails = (await resourcesInfoHandler.HandleAsync(context.Message.MatchId, resourceConditions))
            .ToDictionary(x => x.SourceResource, x => x);

        var conditions = resourceConditions
            .GroupBy(x => new { x.ResourceId, x.RuleName })
            .ToDictionary(g => g.Key, g => g.ToList());

        var validResults = new List<string>();
        foreach (var (ruleKey, sourceCondition) in conditions)
        {
            var valid = sourceCondition.All(sc =>
                resultDetails.TryGetValue(sc.SourceResource, out var resultDetail));

            if (valid)
            {
                validResults.Add(ruleKey.ResourceId);
                break;
            }
        }

        var match = new Match
        {
            MatchId = context.Message.MatchId,
            LeagueId = context.Message.LeagueId,
            ResultDetails = []
        };

        await this.insOrUpdAutoPayoutResultHandler.Handle(match, 0);


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