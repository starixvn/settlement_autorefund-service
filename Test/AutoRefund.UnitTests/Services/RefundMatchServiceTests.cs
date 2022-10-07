using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AutoRefund.UnitTests.Services
{
    public class RefundMatchServiceTests
    {
        private readonly IDynamicRepository repository;
        private readonly IRefundMatchService service;
        private readonly ScaleOutSettleService settleService;

        public RefundMatchServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            settleService = Substitute.For<ScaleOutSettleService>();
            var settings = new AppSettingsKey()
            {
                RetryInterval = 1000,
                RetryCount = 15,
                EnvironmentName = "PRO",
                SiteDbServer = new List<string> { "ABC" }
            };
            var appSettingsOptions = Options.Create(settings);

            service = new RefundMatchServiceImp(repository, settleService, appSettingsOptions);
            settleService.ValidateEventAndTicket(Arg.Any<EventTicketRequest>()).Returns(new ResultMessage { ErrorMessage = "" });
        }

        [Fact]
        public async Task Push_MatchTypeIsAutoRefundNormal_AlwaysReturnNotNull()
        {
            // Arrange
            var match = new Match { MatchId = 1 };

            // Act
            var result = await service.Push(match);

            // Assert
            repository.Received().Execute(Arg.Any<RefundPushCriteria>());
            Assert.NotNull(result);
        }
    }
}