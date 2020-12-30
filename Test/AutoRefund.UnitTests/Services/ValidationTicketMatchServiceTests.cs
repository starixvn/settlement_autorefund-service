using AutoRefund.Exceptions;
using AutoRefund.Models;
using AutoRefund.Services;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class ValidationTicketMatchServiceTests
    {
        private readonly IValidationTicketMatchService service;

        public ValidationTicketMatchServiceTests()
        {
            service = new ValidationTicketServiceImp();
        }

        [Fact]
        public void VerifyTicketMatch_MatchIsExistMarkedTicket_ThrowsMarkedTicketException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                IsExistMarkedTicket = true
            };

            // Assert
            Assert.Throws<MarkedTicketException>(() => service.VerifyTicketMatch(matchChecking));
        }

        [Fact]
        public void VerifyTicketMatch_MatchIsExistSettledTicket_ThrowsSettledTicketException()
        {
            // Arrange
            var matchChecking = new MatchChecking
            {
                IsExistSettledTicket = true
            };


            // Assert
            Assert.Throws<SettledTicketException>(() => service.VerifyTicketMatch(matchChecking));
        }
    }
}
