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
        public void Push_MatchTypeIsAutoRefundNormal_ShouldExecuteRightCriteria()
        {
            // Arrange
            var match = new Match { TypeId = MatchType.AutoRefund, MatchId = 1 };
            var criteria = new RefundPushCriteria { MatchId = match.MatchId };

            // Act
            service.Push(match);

            // Assert
            repository.Received().Execute(criteria);
        }
    }
}
