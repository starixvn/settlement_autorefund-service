using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer : IConsumer<ResultsRefunded>
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly GetResourcesTriggerHandler getResourcesTriggerHandler;
    readonly GetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly GetResourcesInfoHandler resourcesInfoHandler;

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        GetResourcesTriggerHandler getResourcesTriggerHandler,
        GetResourcesConditionsHandler getResourcesConditionsHandler,
        GetResourcesInfoHandler resourcesInfoHandler)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
        this.resourcesInfoHandler = resourcesInfoHandler;
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

        var resultDetails = (await resourcesInfoHandler.HandleAsync(resourceConditions))
            .ToDictionary(x => x.SourceResource, x => x);

        //var groupRuleBySourceId = rules.GroupBy(x => x.SourceId)
        //    .ToDictionary(g => g.Key, g => g.ToList());
        var conditions = resourceConditions
            .GroupBy(x => new { x.ResourceId, x.RuleName })
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var (ruleKey, sourceCondition) in conditions)
        {
            var valid = sourceCondition.All(sc =>
                resultDetails.TryGetValue(sc.SourceResource, out var resultDetail)
                && SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(resultDetail));

            if (valid)
            {
                break;
            }
        }

        //var groupRuleBySourceId = rules.GroupBy(x => x.SourceId)
        //    .ToDictionary(g => g.Key, g => g.ToList());

        //// get source result details
        //var resultDetails = new List<ResultDetail>
        //{
        //    new ResultDetail()
        //    {
        //        MatchId = 83727933,
        //        ResourceId = "38101",
        //        HomeValue = "NT",
        //        AwayValue = "",
        //        ResultStatusId = 5,
        //        SettlementStatusId = 1,
        //    },
        //    new ResultDetail()
        //    {
        //        ResourceId = "38201",
        //        HomeValue = "NT",
        //        AwayValue = "",
        //        ResultStatusId = 5,
        //        SettlementStatusId = 1,

        //    }
        //}.ToDictionary(x => x.ResourceId, x => x);


        //foreach (var (sourceId, rulesList) in groupRuleBySourceId)
        //{
        //    var conditions = rulesList
        //        .GroupBy(x => x.RuleName)
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    foreach (var (ruleKey, sourceCondition) in conditions)
        //    {
        //        var valid = sourceCondition.All(sc =>
        //            resultDetails.TryGetValue(sc.SourceResource, out var resultDetail)
        //            && SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(resultDetail));
        //        if (valid)
        //        {
        //            Console.WriteLine(ruleKey);
        //            Console.WriteLine(sourceId);
        //        }
        //    }
        //}

    }

}

//type
//target
//value
//condition