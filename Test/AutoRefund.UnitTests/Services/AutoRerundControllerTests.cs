using AutoRefund.Models;
using AutoRefund.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class AutoRerundControllerTests
    {
        private readonly IMatchPoolService matchPoolService;

        private readonly IMatchRefundController matchRefundController;

        private readonly IAutoRefundController controller;

        public AutoRerundControllerTests()
        {
            matchPoolService = Substitute.For<IMatchPoolService>();
            matchRefundController = Substitute.For<IMatchRefundController>();
            controller = new AutoRefundControllerImp(matchPoolService, matchRefundController);
        }

        [Fact]
        public void ProcessAsync_EmptyMatches_NotInvokeProcessMethod()
        {
            // Arrange
            matchPoolService.PeekNewMatch().Returns(new List<Match>());

            // Act
            controller.ProcessAsync();

            // Assert
            matchRefundController.DidNotReceive().Process(new Match());
        }

        [Fact]
        public void ProcessAsync_HasMatches_InvokeProcessMethod()
        {
            // Arrange
            var matches = new List<Match>
            {
                new Match { MatchId = 123 }
            };

            matchPoolService.PeekNewMatch().Returns(matches);

            // Act
            controller.ProcessAsync();

            // Assert
            matchRefundController.Received().Process(matches[0]);
        }
    }
}
