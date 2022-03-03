using AutoRefund.Exceptions;
using AutoRefund.Models;
using AutoRefund.Services;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class ValidationSettingMatchServiceTests
    {
        private readonly IValidationSettingMatchService service;

        public ValidationSettingMatchServiceTests()
        {
            service = new ValidationSettingMatchServiceImp();
        }

        [Fact]
        public void VerifySettingMatch_MatchIsHtdHold_ThrowsMtdHoldException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 48 //000011
            };

            // Assert
            Assert.Throws<RemarkException>(() => service.VerifySettingMatch(matchChecking));
        }

        [Fact]
        public void VerifySettingMatch_MatchIsUndecided_ThrowsUndecidedException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 96 //0000011
            };
    
            // Assert
            Assert.Throws<RemarkException>(() => service.VerifySettingMatch(matchChecking));
        }

        [Fact]
        public void VerifySettingMatch_MatchIsPending_ThrowsPendingException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 33, //100001
            };

            // Assert
            Assert.Throws<RemarkException>(() => service.VerifySettingMatch(matchChecking));
        }

        [Fact]
        public void VerifySettingMatch_MatchIsUnderSettle_ThrowsUnderSettleException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 36, // 001001
            };

            // Assert
            Assert.Throws<RemarkException>(() => service.VerifySettingMatch(matchChecking));
        }

        [Fact]
        public void VerifySettingMatch_MatchIsHoldKey_ThrowsHoldKeyException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 34, //010001
            };

            // Assert
            Assert.Throws<RemarkException>(() => service.VerifySettingMatch(matchChecking));
        }

        [Fact]
        public void VerifySettingMatch_MatchIsHoldResourceId_ThrowsHoldKeyException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 32, //0000010
                IsHoldResourceId = true
            };

            // Assert
            Assert.Throws<RemarkException>(() => service.VerifySettingMatch(matchChecking));
        }

        [Fact]
        public void VerifySettingMatch_RefundFlagIsOff_ThrowsRefundException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 0, //0000000
            };

            // Assert
            Assert.Throws<RefundAllException>(() => service.VerifySettingMatch(matchChecking));
        }

        [Fact]
        public void VerifySettingMatch_PassAllMatchChecking_DoesNotThrowsRefundException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                SettlementStatus = 32,
                IsHoldResourceId = false
            };

            // Assert
            var exception = Record.Exception(() => service.VerifySettingMatch(matchChecking));
            Assert.Null(exception);
        }
    }
}
