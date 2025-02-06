using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer : IConsumer<ResultsRefunded>
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly GetResourcesTriggerHandler getResourcesTriggerHandler;
    readonly GetResourcesConditionsHandler getResourcesConditionsHandler;

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        GetResourcesTriggerHandler getResourcesTriggerHandler,
        GetResourcesConditionsHandler getResourcesConditionsHandler)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.getResourcesConditionsHandler = getResourcesConditionsHandler;
    }

    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        logger.LogInformation("Refund result received: {MatchId}", context.Message.MatchId);
        //var x = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(new GetResourcesTriggerCriteria
        //{
        //    Action = context.Message.Action,
        //    SportType = context.Message.ShortType,
        //    ChildMatchType = context.Message.ChildMatchType,
        //    ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
        //});

        //var y = await getResourcesConditionsHandler.HandleAsync(
        //    context.Message.ShortType,
        //    2,
        //    context.Message.ChildMatchType,
        //    x
        //    );
        List<string> resourcesTrigger = ["38301", "38401"];

        var rules = new List<SourcesConditions>
{
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule1", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38101", Conditions = "Refunded" },
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule1", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38201", Conditions = "Refunded" },
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule2", Action = "Cal", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38101", Conditions = "0" },
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule2", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38201", Conditions = "0" },
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule2", Action = "CfnRefundSDT", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38301", Conditions = "0" },
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule3", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38101", Conditions = "0" },
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule3", Action = "Cal", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38201", Conditions = "0" },
    new() { Id = 77, SportType = 1, SourceId = "38301", MatchType = 2, ChildMatchType = 202, RuleName = "Rule3", Action = "CfnRefundSDT", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38301", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule1", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38101", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule1", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38201", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule2", Action = "Cal", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38101", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule2", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38201", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule2", Action = "CfnRefundSDT", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38401", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule3", Action = "Refund", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38101", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule3", Action = "Cal", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38201", Conditions = "0" },
    new() { Id = 82, SportType = 1, SourceId = "38401", MatchType = 2, ChildMatchType = 202, RuleName = "Rule3", Action = "CfnRefundSDT", SourceMatchType = 1, SourceChildMatchType = 202, SourceResource = "38401", Conditions = "0" }
};


        var groupRuleBySourceId = rules.GroupBy(x => x.SourceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // get source result details
        var resultDetails = new List<ResultDetail>
        {
            new ResultDetail()
            {
                MatchId = 83727933,
                ResourceId = "38101",
                HomeValue = "NT",
                AwayValue = "",
                ResultStatusId = 5,
                SettlementStatusId = 1,
            },
            new ResultDetail()
            {
                ResourceId = "38201",
                HomeValue = "NT",
                AwayValue = "",
                ResultStatusId = 5,
                SettlementStatusId = 1,

            }
        }.ToDictionary(x => x.ResourceId, x => x);


        foreach (var (sourceId, rulesList) in groupRuleBySourceId)
        {
            var conditions = rulesList
                .GroupBy(x => x.RuleName)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (ruleKey, sourceCondition) in conditions)
            {
                var valid = sourceCondition.All(sc =>
                    resultDetails.TryGetValue(sc.SourceResource, out var resultDetail)
                    && SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(resultDetail));
                if (valid)
                {
                    Console.WriteLine(ruleKey);
                    Console.WriteLine(sourceId);
                }
            }
        }

        //foreach (var rule in groupRules)
        //{
        //    var valid = rule.Value.All(sc =>
        //            resultDetails.TryGetValue(sc.SourceResource, out var resultDetail)
        //            && SpecificationFactory.CreateSpecification(sc).IsSatisfiedBy(resultDetail));

        //    if (valid)
        //    {
        //        Console.WriteLine(rule.Key);
        //        Console.WriteLine();
        //    }

        //}
    }

}

//type
//target
//value
//condition