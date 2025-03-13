using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Features.RefundConsumerHandler;
using AutoEarlyRefund.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AutoEarlyRefund.Consumers;

public sealed class RefundConsumer : IConsumer<ResultsRefunded>, IConsumer<ResultsRefundConfirmedSdt>,
    IConsumer<MatchCalculated>, IConsumer<ResultsCalculated>, IConsumer<ResultsRefundConfirmed>, IConsumer<MatchRefunded>, IConsumer<NotTakenResultsConfirmed>
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly GetResourcesTriggerHandler getResourcesTriggerHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler;
    private readonly RefundConsumerHandler refundConsumerHandler;

    public RefundConsumer(
        ILogger<RefundConsumer> logger,
        GetResourcesTriggerHandler getResourcesTriggerHandler,
        IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler,
        XmlConvertService xmlConvertService,
        RefundConsumerHandler refundConsumerHandler)
    {
        this.logger = logger;
        this.getResourcesTriggerHandler = getResourcesTriggerHandler;
        this.insOrUpdAutoPayoutResultHandler = insOrUpdAutoPayoutResultHandler;
        this.refundConsumerHandler = refundConsumerHandler;
    }

    public async Task Consume(ConsumeContext<ResultsRefunded> context)
    {
        logger.LogInformation($"ResultsRefunded Received: {context.Message}");

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
    }

    public async Task Consume(ConsumeContext<ResultsRefundConfirmedSdt> context)
    {
        logger.LogInformation($"ResultsRefundConfirmedSdt Received: {context.Message}");

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

        logger.LogInformation($"ResultsRefundConfirmedSdt Done: {context.Message}");
    }


    public async Task Consume(ConsumeContext<MatchCalculated> context)
    {
        logger.LogInformation($"""MatchCalculated Received: {context.Message}""");

        if (!ConsumerHelper.IsExtraMatchCalculated(context.Message) && !ConsumerHelper.IsPenMatchCalculated(context.Message))
        {
            return;
        }

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            "00");

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"MatchCalculated Done: {context.Message}");
    }



    public async Task Consume(ConsumeContext<ResultsCalculated> context)
    {
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
    }


    public async Task Consume(ConsumeContext<ResultsRefundConfirmed> context)
    {
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
    }

    public async Task Consume(ConsumeContext<MatchRefunded> context)
    {
        logger.LogInformation($"MatchRefunded Received: {context.Message}");

        if (!ConsumerHelper.IsExtraMatchRefunded(context.Message))
        {
            return;
        }

        var triggerResources = await getResourcesTriggerHandler.GetTargetResourceTriggersAsync(
            context.Message.Action,
            context.Message.ShortType,
            context.Message.ChildMatchType,
            "00");

        var (isValid, match) = await refundConsumerHandler.Handle(context.Message.MatchId, triggerResources);

        if (isValid)
        {
            await insOrUpdAutoPayoutResultHandler.Handle(match, 0);
        }

        logger.LogInformation($"MatchRefunded Done: {context.Message}");
    }

    public async Task Consume(ConsumeContext<NotTakenResultsConfirmed> context)
    {
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
    }
}