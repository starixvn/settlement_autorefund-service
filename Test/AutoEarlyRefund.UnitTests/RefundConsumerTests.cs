using AutoEarlyRefund.Consumers;
using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Features.GetRefundMatchInfoSel;
using AutoEarlyRefund.Features.GetResourcesConditions;
using AutoEarlyRefund.Features.GetResourcesInfo;
using AutoEarlyRefund.Features.GetResourcesTrigger;
using AutoEarlyRefund.Features.InsOrUpdAutoPayoutResult;
using AutoEarlyRefund.Models;
using AutoRefund.Core.Models;
using Fanex.Settlement.Converter.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

public class RefundConsumerTests
{
    private readonly ILogger<RefundConsumer> logger;
    private readonly GetResourcesTriggerHandler getResourcesTriggerHandler;
    private readonly GetResourcesConditionsHandler getResourcesConditionsHandler;
    private readonly GetResourcesInfoHandler resourcesInfoHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResultHandler;
    private readonly XmlConvertService xmlConvertService;
    private readonly GetRefundMatchInfoHandler getRefundMatchInfoHandler;
    private readonly RefundConsumer refundConsumer;

    public RefundConsumerTests()
    {
        logger = Substitute.For<ILogger<RefundConsumer>>();
        getResourcesTriggerHandler = Substitute.For<GetResourcesTriggerHandler>();
        getResourcesConditionsHandler = Substitute.For<GetResourcesConditionsHandler>();
        resourcesInfoHandler = Substitute.For<GetResourcesInfoHandler>();
        insOrUpdAutoPayoutResultHandler = Substitute.For<IInsOrUpdAutoPayoutResultHandler>();
        xmlConvertService = Substitute.For<XmlConvertService>();
        getRefundMatchInfoHandler = Substitute.For<GetRefundMatchInfoHandler>();

        refundConsumer = new RefundConsumer(
            logger,
            getResourcesTriggerHandler,
            getResourcesConditionsHandler,
            resourcesInfoHandler,
            insOrUpdAutoPayoutResultHandler,
            xmlConvertService,
            getRefundMatchInfoHandler);
    }

    [Fact]
    public async Task Consume_ResultsRefunded_ShouldLogInformation()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<ResultsRefunded>>();
        var message = new ResultsRefunded(1, 1, 1, new List<ResourceItem> { new ResourceItem { ResourceId = "1" } });
        context.Message.Returns(message);

        getResourcesTriggerHandler
            .GetTargetResourceTriggersAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        await refundConsumer.Consume(context);

        // Assert
        logger.Received(1).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task Consume_ResultsRefundConfirmedSdt_ShouldLogInformation()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<ResultsRefundConfirmedSdt>>();
        var message = new ResultsRefundConfirmedSdt(1, 1, 1, new List<ResourceItem> { new ResourceItem { ResourceId = "1" } });
        context.Message.Returns(message);

        getResourcesTriggerHandler
            .GetTargetResourceTriggersAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        await refundConsumer.Consume(context);

        // Assert
        logger.Received(2).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task Consume_ResultsRefundConfirmed_ShouldLogInformation()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<ResultsRefundConfirmed>>();
        var message = new ResultsRefundConfirmed(1, 1, 1, new List<ResourceItem> { new ResourceItem { ResourceId = "1" } });
        context.Message.Returns(message);

        getResourcesTriggerHandler
            .GetTargetResourceTriggersAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        await refundConsumer.Consume(context);

        // Assert
        logger.Received(2).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task Consume_MatchCalculated_ShouldLogInformation()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<MatchCalculated>>();
        var message = new MatchCalculated(1, 1, 1, 1, null, null, null, null, "1");
        context.Message.Returns(message);

        getResourcesTriggerHandler
            .GetTargetResourceTriggersAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        await refundConsumer.Consume(context);

        // Assert
        logger.Received(2).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task Consume_MainMatchCalculated_ShouldLogInformation()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<MainMatchCalculated>>();
        var message = new MainMatchCalculated(1, 1, 1, 1, null, null, null, null, "1");
        context.Message.Returns(message);

        getResourcesTriggerHandler
            .GetTargetResourceTriggersAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        await refundConsumer.Consume(context);

        // Assert
        logger.Received(2).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task Consume_ExtraMatchCalculated_ShouldLogInformation()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<ExtraMatchCalculated>>();
        var message = new ExtraMatchCalculated(1, 1, 1, 1, null, null, null, null, "1");
        context.Message.Returns(message);

        getResourcesTriggerHandler
            .GetTargetResourceTriggersAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        await refundConsumer.Consume(context);

        // Assert
        logger.Received(2).LogInformation(Arg.Any<string>());
    }

    [Fact]
    public async Task Consume_ResultsCalculated_ShouldLogInformation()
    {
        // Arrange
        var context = Substitute.For<ConsumeContext<ResultsCalculated>>();
        var message = new ResultsCalculated(1, 1, 1, new List<ResourceItem> { new ResourceItem { ResourceId = "1" } });
        context.Message.Returns(message);

        getResourcesTriggerHandler
            .GetTargetResourceTriggersAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<short>(), Arg.Any<string>())
            .Returns(new List<TargetResourceTrigger>());

        // Act
        await refundConsumer.Consume(context);

        // Assert
        logger.Received(2).LogInformation(Arg.Any<string>());
    }
}
