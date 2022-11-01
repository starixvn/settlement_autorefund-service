using AutoRefund.Models;
using AutoRefund.Services;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class AutoRefundControllerTests
{
    private readonly IMatchPoolService matchPoolService;

    private readonly IMatchRefundController matchRefundController;

    private readonly IAutoRefundController controller;


    public AutoRefundControllerTests()
    {
        matchPoolService = Substitute.For<IMatchPoolService>();
        matchRefundController = Substitute.For<IMatchRefundController>();
        controller = new AutoRefundControllerImp(matchPoolService, matchRefundController);
    }

    [Fact]
    public async Task ProcessAsync_HasMatches_InvokeProcessMethod()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var matches = new List<Match> { match };

        matchPoolService.PeekNewMatch().Returns(matches);

        // Act
        await controller.ProcessAsync();
        Thread.Sleep(1000);

        // Assert
        await matchRefundController.Received().Process(match);
    }
}
