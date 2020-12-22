using AutoRefund.Models;
using AutoRefund.Services;
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
            var match = new Models.Match { NumberOfRetries = 0 };
            validationService.VerifyMatchBeforeExecution(match).Returns(new Result { ErrCode = 1 });

            // Act
            service.Process(match);

            // Assert
            Received.InOrder(() =>
            {
                logService.DidNotReceive().ActionLog(match, Arg.Any<string>());
                matchPoolService.DidNotReceive().Remove(match);
            });
        }
    }
}