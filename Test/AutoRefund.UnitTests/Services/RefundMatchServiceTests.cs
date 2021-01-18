using AutoRefund.Constants;
using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class RefundMatchServiceTests
    {
        private readonly IDynamicRepository repository;

        private readonly IRefundMatchService service;

        public RefundMatchServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            service = new RefundMatchServiceImp(repository);
        }

        [Fact]
        public void Push_MatchTypeIsAutoRefundNormal_AlwaysReturnNotNull()
        {
            // Arrange
            var match = new Match { TypeId = MatchType.RefundSingle, MatchId = 1 };

            // Act
            var result = service.Push(match);

            // Assert
            repository.Received().Execute(Arg.Any<RefundPushCriteria>());
            Assert.NotNull(result);
        }
    }
}
