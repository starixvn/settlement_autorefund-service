

using AutoRefund.Constants;
using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class ValidationServiceTests
    {
        private readonly IDynamicRepository repository;

        private readonly IValidationService service;

        public ValidationServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            service = new ValidationServiceImp(repository);
        }


        [Fact]
        public void Push_Always_ReturnNotNull()
        {
            // Arrange
            var match = new Match { TypeId = MatchType.AutoRefund, MatchId = 1 };

            // Act
            var result = service.VerifyMatchBeforeExecution(match);

            // Assert
            repository.Received().Execute(Arg.Any<MatchCheckingCriteria>());
            Assert.NotNull(result);
        }
    }
}
