using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.RefundConsumerHandler;
using AutoEarlyRefund.Helpers;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.TriggerConditions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer : IConsumer<ResultsRefunded>,
    IConsumer<ResultsRefundConfirmedSdt>,
    IConsumer<MatchCalculated>,
    IConsumer<ResultsCalculated>,
    IConsumer<ResultsRefundConfirmed>,
    IConsumer<MatchRefunded>,
    IConsumer<NotTakenResultsConfirmed>
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly IGetResourcesTriggerHandler getResourcesTriggerHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler;
    private readonly IRefundConsumerHandler refundConsumerHandler;

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        IGetResourcesTriggerHandler getResourcesTriggerHandler,
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
        XmlConvertService xmlConvertService,
        IRefundConsumerHandler refundConsumerHandler)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.insOrUpdAutoPayoutResultHandler = insOrUpdAutoPayoutResultHandler;
        this.refundConsumerHandler = refundConsumerHandler;
    }

    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        logger.LogInformation($"ResultsRefunded Received: {context.Message}");
        Fanex.Logging.LogService.Info($"ResultsRefunded Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"ResultsRefunded Done: {context.Message}");
        Fanex.Logging.LogService.Info($"ResultsRefunded Done: {context.Message}");
    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmedSdt> context)
    {
        Fanex.Logging.LogService.Info($"ResultsRefundConfirmedSdt Received: {context.Message}");
        logger.LogInformation($"ResultsRefundConfirmedSdt Received: {context.Message}");

        var hasValidResource = ConsumerHelper.TryGetValidResources(context.Message.Resources, out var validResources);

        if (!hasValidResource)
        {
            return;
        }

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", validResources.Select(x => x.ResourceId)));

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"ResultsRefundConfirmedSdt Done: {context.Message}");
        Fanex.Logging.LogService.Info($"ResultsRefundConfirmedSdt Done: {context.Message}");
    }


    public async Task Consume(ConsumeContext<MatchCalculated> context)
    {
        Fanex.Logging.LogService.Info($"MatchCalculated Received: {context.Message}");
        logger.LogInformation($"""MatchCalculated Received: {context.Message}""");

        var newMatchCalculated = context.Message with { Action = GetAction(context.Message.ActionName) };

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            newMatchCalculated.Action,
            newMatchCalculated.ShortType,
            newMatchCalculated.ChildMatchType,
            "00");

        // TODO
        var validTriggerResources = TryGetValidTargetResourceTrigger(triggerResources, context.Message);

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, validTriggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"MatchCalculated Done: {context.Message}");
        Fanex.Logging.LogService.Info($"MatchCalculated Done: {context.Message}");
    }



    public async Task Consume(ConsumeContext<ResultsCalculated> context)
    {
        Fanex.Logging.LogService.Info($"ResultsCalculated Received: {context.Message}");
        logger.LogInformation($"ResultsCalculated Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"ResultsCalculated Done: {context.Message}");
        Fanex.Logging.LogService.Info($"ResultsCalculated Done: {context.Message}");
    }


    public async Task Consume(ConsumeContext<ResultsRefundConfirmed> context)
    {
        Fanex.Logging.LogService.Info($"ResultsRefundConfirmed Received: {context.Message}");
        logger.LogInformation($"ResultsRefundConfirmed Received: {context.Message}");

        var hasValidResource = ConsumerHelper.TryGetValidResources(context.Message.Resources, out var validResources);

        if (!hasValidResource) return;

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", validResources.Select(x => x.ResourceId)));

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"ResultsRefundConfirmed Done: {context.Message}");
        Fanex.Logging.LogService.Info($"ResultsRefundConfirmed Done: {context.Message}");
    }

    public async Task Consume(ConsumeContext<MatchRefunded> context)
    {
        Fanex.Logging.LogService.Info($"MatchRefunded Received: {context.Message}");
        logger.LogInformation($"MatchRefunded Received: {context.Message}");

        var newMatchRefunded = context.Message with { Action = GetAction(context.Message.ActionName) };
        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            newMatchRefunded.Action,
           newMatchRefunded.ShortType,
            newMatchRefunded.ChildMatchType,
            "00");

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"MatchRefunded Done: {context.Message}");
        Fanex.Logging.LogService.Info($"MatchRefunded Done: {context.Message}");
    }

    public async Task Consume(ConsumeContext<NotTakenResultsConfirmed> context)
    {
        Fanex.Logging.LogService.Info($"NotTakenResultsConfirmed Received: {context.Message}");
        logger.LogInformation($"NotTakenResultsConfirmed Received: {context.Message}");

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            string.Join(",", context.Message.Resources.Select(x => x.ResourceId)));

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"NotTakenResultsConfirmed Done: {context.Message}");
        Fanex.Logging.LogService.Info($"NotTakenResultsConfirmed Done: {context.Message}");
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
        if (actionName == "REFUND-FT" || actionName == "REFUND")
        {
            return "Refund";
        }

        if (actionName == "CAL" || actionName == "CAL-FT")
        {
            return "Cal";
        }

        return actionName;
    }
}