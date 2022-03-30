using System.Threading.Tasks;
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
        public async Task Push_MatchTypeIsAutoRefundNormal_AlwaysReturnNotNull()
        {
            // Arrange
            var match = new Match { MatchId = 1 };

            // Act
            var result = await service.Push(match);

            // Assert
            await repository.Received().ExecuteAsync(Arg.Any<RefundPushCriteria>());
            Assert.NotNull(result);
        }
    }
}