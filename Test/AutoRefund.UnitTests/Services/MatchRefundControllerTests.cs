using AutoRefund.Constants;
using AutoRefund.Exceptions;
using AutoRefund.Models;
using AutoRefund.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Threading;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class MatchRefundControllerTests
    {
        private readonly IMatchPoolService matchPoolService;

        private readonly IValidationService validationService;

        private readonly IRefundMatchService refundMatchService;

        private readonly IMatchService matchService;

        private readonly ILogService logService;

        private readonly IMatchRefundController service;

        public MatchRefundControllerTests()
        {
            matchPoolService = Substitute.For<IMatchPoolService>();
            validationService = Substitute.For<IValidationService>();
            refundMatchService = Substitute.For<IRefundMatchService>();
            matchService = Substitute.For<IMatchService>();
            logService = Substitute.For<ILogService>();

            var settings = new AppSettingsKey()
            {
                RetryInterval = 1000,
                RetryCount = 15
            };

            IOptions<AppSettingsKey> appSettingsOptions = Options.Create(settings);
            service = new MatchRefundControllerImp(matchPoolService, validationService, refundMatchService, matchService, logService, appSettingsOptions);
        }

        [Fact]
        public void Process_NoExceptionHaveBeenThrown_PushMatchSucceedAndRemoveMatch()
        {
            // Arrange
            var match = new Match();
            validationService.When(fake => fake.VerifyMatchBeforeExecution(match)).Do(call => { });
            refundMatchService.Push(match).Returns(new Result());

            // Act
            service.Process(match);

            // Assert
            refundMatchService.Received().Push(match);
            matchPoolService.Received().RemoveMatch(match);
            matchService.DidNotReceive().UpdateRemark(match, "failed", MatchStatus.Failed);
        }

        [Fact]
        public void Process_NoExceptionHaveBeenThrown_PushMatchFailedAndUpdateStatus()
        {
            // Arrange
            var match = new Match();
            validationService.When(fake => fake.VerifyMatchBeforeExecution(match)).Do(call => { });
            refundMatchService.Push(match).Returns(new Result { ErrMsg = "failed" });

            // Act
            service.Process(match);

            // Assert
            refundMatchService.Received().Push(match);
            logService.Received().ActionLog(match, "failed");
            matchService.Received().UpdateRemark(match, "failed", MatchStatus.Failed);
        }

        [Fact]
        public void Process_ThrowExceptionOnVerifyMatchAndTimeMax_InvokeRelatedMethods()
        {
            // Arrange
            var match = new Match();
            validationService.When(fake => fake.VerifyMatchBeforeExecution(match)).Do(call =>
            {
                throw new AutoRefundException();
            });

            // Act
            service.Process(match);

            // Assert
            matchService.Received(16).UpdateRemark(Arg.Any<Match>(), Arg.Any<string>(), Arg.Any<int>());
            logService.Received().ActionLog(match, Arg.Any<string>());
            matchPoolService.Received().RemoveAndUpdateStatus(match, Arg.Any<int>());
        }
    }
}