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
        var x = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(new GetResourcesTriggerCriteria
        {
            Action = context.Message.Action,
            SportType = context.Message.ShortType,
            ChildMatchType = context.Message.ChildMatchType,
            ResourceIds = string.Join(",", context.Message.Resources.Select(x => x.ResourceId))
        });

        var y = await getResourcesConditionsHandler.HandleAsync(
            context.Message.ShortType,
            2,
            context.Message.ChildMatchType,
            x
            );


        var groupRules = y.GroupBy(x => x.RuleName);

        foreach (var item in groupRules)
        {
            var resourceIds = item.Select(x => x.SourceResource).ToList();
            var resultDetails = new List<ResultDetail>();

            bool flag = false;
            foreach (var condition in item.ToList())
            {
                if (condition.Conditions == "Refunded")
                {
                    // GET REsources
                    // Check if the source is already refunded
                    var z = resultDetails
                        .Where(x => x.ResourceId == condition.SourceResource && x.SettlementStatusId == (short)SettleStatus.Refunded);

                }
            }
        }
    }

}

