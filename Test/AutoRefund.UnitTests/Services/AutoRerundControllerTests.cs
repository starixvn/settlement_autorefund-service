using AutoRefund.Models;
using AutoRefund.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class AutoRerundControllerTests
    {
        [Fact]
        public void ProcessAsync_HasMatches_InvokeProcessMethod()
        {
            var matchPoolService = Substitute.For<IMatchPoolService>();
            var matchRefundController = Substitute.For<IMatchRefundController>();
            var controller = new AutoRefundControllerImp(matchPoolService, matchRefundController);
            
            // Arrange
            var matches = new List<Match>
            {
                new Match { MatchId = 123 , IsChecking = false }
            };

            matchPoolService.PeekNewMatch().Returns(matches);

            // Act
            controller.ProcessAsync();

            // Assert
            matchRefundController.Received().Process(matches[0]);
        }
    }
}
