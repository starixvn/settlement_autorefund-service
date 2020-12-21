using AutoRefund.Exceptions;
using AutoRefund.Models;
using AutoRefund.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class MatchRefundControllerTests
    {
        private readonly IMatchPoolService matchPoolService;

        private readonly IValidationService validationService;

        private readonly IRefundMatchService refundMatchService;

        private readonly ILogService logService;

        private readonly IMatchRefundController service;

        public MatchRefundControllerTests()
        {
            matchPoolService = Substitute.For<IMatchPoolService>();
            validationService = Substitute.For<IValidationService>();
            refundMatchService = Substitute.For<IRefundMatchService>();
            logService = Substitute.For<ILogService>();
            service = new MatchRefundControllerImp(matchPoolService, validationService, refundMatchService, logService);
        }

        [Fact]
        public void Process_ThrowExceptionOnVerifyMatchButRetryNotEnough_NotInvokeRelatedMethods()
        {
            // Arrange
            var match = new Models.Match { finalawayscore = 1, finalhomescore = 1, NumberOfRetries = 0};
            validationService.When(fake => fake.VerifyMatchBeforeExecution(match)).Do(call => { throw new AutoRefundException(); });

            // Act
            service.Process(match);

            // Assert
            Received.InOrder(() =>
            {
                logService.DidNotReceive().ActionLog(match, Arg.Any<AutoRefundException>());
                matchPoolService.DidNotReceive().Remove(match);
            });
        }

    }
}
