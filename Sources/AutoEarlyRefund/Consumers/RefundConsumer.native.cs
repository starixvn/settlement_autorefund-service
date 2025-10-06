using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoRefundResult;
using AutoEarlyRefund.Features.RefundConsumerHandler;
using AutoEarlyRefund.Helpers;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.TriggerConditions;
using Fanex.Logging;
using Fanex.Settlement.MessageBridge.RefundEvents;
using MassTransit;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer(
    IGetResourcesTriggerHandler getResourcesTriggerHandler,
    IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
    IRefundConsumerHandler refundConsumerHandler)
    : IConsumer<ResultsRefunded>,
        IConsumer<ResultsRefundConfirmedSdt>,
        IConsumer<MatchCalculated>,
        IConsumer<ResultsCalculated>,
        IConsumer<ResultsRefundConfirmed>,
        IConsumer<MatchRefunded>,
        IConsumer<NotTakenResultsConfirmed>
{
    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            "Refund",
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        var (isValid, matches) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(matches, 0);
        }
    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmedSdt> context)
    {
        var hasValidResource = ConsumerHelper.TryGetValidResources(context.Message.Resources, out var validResources);

        if (!hasValidResource)
        {
            return;
        }

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            "CfmRefundSDT",
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", validResources.Select(x => x.ResourceId)));

        var (isValid, matches) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(matches, 0);
        }
    }

    public async Task Consume(ConsumeContext<MatchCalculated> context)
    {
        var action = GetAction(context.Message.Action);

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            "00");

        var validTriggerResources = TryGetValidTargetResourceTrigger(triggerResources, context.Message);

        var (isValid, matches) = await refundConsumerHandler.Handle(context.Message.MatchId, validTriggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(matches, 0);
        }
    }

    public async Task Consume(ConsumeContext<ResultsCalculated> context)
    {
        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
           "Cal",
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        var (isValid, matches) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(matches, 0);
        }
    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmed> context)
    {
        var hasValidResource = ConsumerHelper.TryGetValidResources(context.Message.Resources, out var validResources);

        if (!hasValidResource) return;

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            "CfmRefund",
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", validResources.Select(x => x.ResourceId)));

        var (isValid, matches) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(matches, 0);
        }
    }

    public async Task Consume(ConsumeContext<MatchRefunded> context)
    {
        var action = GetAction(context.Message.Action);
        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            action,
           context.Message.ShortType,
            context.Message.ChildMatchType,
            "00");

        var (isValid, matches) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(matches, 0);
        }
    }

    public async Task Consume(ConsumeContext<NotTakenResultsConfirmed> context)
    {
        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            "CfmRefundAdminSite",
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        var (isValid, matches) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(matches, 0);
        }
    }

    private List<TargetResourceTrigger> TryGetValidTargetResourceTrigger(
        List<TargetResourceTrigger> resourceTriggers,
        MatchCalculated matchCalculated)
    {
        var rules = new Dictionary<string, ITriggerConditionRule>
        {
            { "1", new WinLoseCondition() },
            { "", new DefaultCondition() }
        };

        return [.. resourceTriggers
            .Where(trigger => !rules.TryGetValue(trigger.Conditions, out _) || rules.TryGetValue(trigger.Conditions, out var rule) && rule.IsValid(matchCalculated.FinalHomeScore ?? 0, matchCalculated.FinalAwayScore ?? 0, trigger))];
    }

    private static string GetAction(string actionName)
    {
        return actionName switch
        {
            "REFUND-FT" or "REFUND" => "Refund",
            "CAL" or "CAL-FT" => "Cal",
            _ => actionName
        };
    }
}