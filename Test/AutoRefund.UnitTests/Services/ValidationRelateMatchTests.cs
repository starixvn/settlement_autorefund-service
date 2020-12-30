using AutoRefund.Exceptions;
using AutoRefund.Models;
using AutoRefund.Services;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class ValidationRelateMatchTests
    {
        private readonly IValidationRelateMatchService service;
        public ValidationRelateMatchTests()
        {
            service = new ValidationRelateMatchServiceImp();
        }

        [Fact]
        public void VerifyChildMatchIsRefudedSingle_MatchNotRefundSingle_ThrowException()
        {
            // Arrange
            var matchChecking = new MatchChecking { IsRefundedSingle = false };

            // Assert
            Assert.Throws<MatchSingleRefundedException>(() => service.VerifyChildMatchIsRefudedSingle(matchChecking));
        }

        [Fact]
        public void VerifyRelateMatch_MatchIsSettled_ThrowException()
        {
            // Arrange
            var matchChecking = new MatchChecking { EventStatus = "completed" };

            // Assert
            Assert.Throws<CurrentMatchIsSettledException>(() => service.VerifyRelateMatch(matchChecking));
        }

        [Fact]
        public void VerifyRelateMatch_MainMatchIsNotSettled_ThrowException()
        {
            // Arrange
            var matchChecking = new MatchChecking { ParentSettlementStatusID = 1, EventStatus = "running" };

            // Assert
            Assert.Throws<MainMatchNotCompletedException>(() => service.VerifyRelateMatch(matchChecking));
        }

        [Fact]
        public void VerifyRelateMatch_MainIsDraw_ThrowException()
        {
            // Arrange
            var matchChecking = new MatchChecking { ParentSettlementStatusID = 4, EventStatus = "running", ParentAwayValue = 1, ParentHomeValue = 1 };

            // Assert
            Assert.Throws<MainMatchNotDecisiveException>(() => service.VerifyRelateMatch(matchChecking));
        }
    }
}
