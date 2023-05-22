using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Models;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class RefundMatchServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly IRefundMatchService service;

    public RefundMatchServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        var settleService = Substitute.For<ScaleOutSettleService>();
        var logDirectlyService = Substitute.For<LogService>();
        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 15,
            EnvironmentName = "PRO",
            SiteDbServer = new List<string> { "ABC" }
        };
        var appSettingsOptions = Options.Create(settings);

        service = new RefundMatchServiceImp(repository, settleService, appSettingsOptions, logDirectlyService);
        settleService.ValidateEventAndTicket(Arg.Any<EventTicketRequest>()).Returns(new ResultMessage { ErrorMessage = "" });
    }

    [Fact]
    public void Push_MatchTypeIsAutoRefundNormal_AlwaysReturnNotNull()
    {
        // Arrange
        var match = new Match { MatchId = 1 };

        // Act
        var result = service.Push(match);

        // Assert
        repository.Received().Execute(Arg.Any<RefundPushCriteria>());
        Assert.NotNull(result);
    }
}