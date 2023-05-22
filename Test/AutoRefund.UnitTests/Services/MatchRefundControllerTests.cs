using AutoRefund.Exceptions;
using AutoRefund.Models;
using AutoRefund.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class MatchRefundControllerTests
{
    private readonly IMatchPoolService matchPoolService;

    private readonly IValidationService validationService;

    private readonly IRefundMatchService refundMatchService;

    private readonly IMatchService matchService;

    private readonly LogService logService;

    private readonly IMatchRefundController service;

    public MatchRefundControllerTests()
    {
        matchPoolService = Substitute.For<IMatchPoolService>();
        validationService = Substitute.For<IValidationService>();
        refundMatchService = Substitute.For<IRefundMatchService>();
        matchService = Substitute.For<IMatchService>();
        logService = Substitute.For<LogService>();

        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 15
        };

        var appSettingsOptions = Options.Create(settings);
        service = new MatchRefundControllerImp(matchPoolService, validationService, refundMatchService, matchService, logService, appSettingsOptions);
    }

    [Fact]
    public async Task Process_ThrowExceptionOnVerifyMatchAndTimeMax_InvokeRelatedMethods()
    {
        // Arrange
        var match = new Match();
        validationService.When(fake => fake.VerifyMatchBeforeExecution(match)).Do(call =>
        {
            throw new AutoRefundException();
        });

        // Act
        await service.Process(match);

        // Assert
        await matchService.Received(16).UpdateRemark(Arg.Any<Match>(), Arg.Any<string>(), Arg.Any<int>());
        await logService.Received().ActionLog(match, Arg.Any<string>());
        matchPoolService.Received().RemoveAndUpdateStatus(match, Arg.Any<int>());
    }
}