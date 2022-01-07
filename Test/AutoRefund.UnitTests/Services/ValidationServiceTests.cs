

using AutoRefund.Constants;
using AutoRefund.Models;
using AutoRefund.Services;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class ValidationServiceTests
    {

        private readonly IValidationSettingMatchService validationSettingMatchService;

        private readonly IValidationTicketMatchService validationTicketMatchService;

        private readonly IValidationRelateMatchService validationRelateMatchService;

        private readonly IMatchService matchService;

        private readonly IValidationService service;

        public ValidationServiceTests()
        {
            matchService = Substitute.For<IMatchService>();
            validationSettingMatchService = Substitute.For<IValidationSettingMatchService>();
            validationTicketMatchService = Substitute.For<IValidationTicketMatchService>();
            validationRelateMatchService = Substitute.For<IValidationRelateMatchService>();
            service = new ValidationServiceImp(validationSettingMatchService, validationTicketMatchService, validationRelateMatchService, matchService);
        }

        [Fact]
        public void VerifyMatchBeforeExecution_MatchIsRefundSingle_InvokeThreeMethods()
        {
            // Arrange
            var match = new Match { TypeId = MatchType.RefundSingle };

            // Act
            service.VerifyMatchBeforeExecution(match);

            // Assert
            validationRelateMatchService.Received().VerifyRelateMatch(Arg.Any<MatchChecking>());
            validationSettingMatchService.Received().VerifySettingMatch(Arg.Any<MatchChecking>());
            validationTicketMatchService.Received().VerifyTicketMatch(Arg.Any<MatchChecking>());

        }

        [Fact]
        public void VerifyMatchBeforeExecution_MatchIsRefundParlay_InvokeTwoMethods()
        {
            // Arrange
            var match = new Match { TypeId = MatchType.RefundParlay};

            // Act
            service.VerifyMatchBeforeExecution(match);

            // Assert
            validationRelateMatchService.Received().VerifyChildMatchIsRefundSingle(Arg.Any<MatchChecking>());
            validationTicketMatchService.Received().CheckMarkedTicket(Arg.Any<Match>());

        }
    }
}
