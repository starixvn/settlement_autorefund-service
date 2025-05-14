using AutoRefund.Core.Models;
using AutoRefund.Criteria;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.Converter.Services;
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
    private readonly ScaleOutSettleService scaleOutSettleService;
    private readonly RefundMatchServiceImp service;

    public RefundMatchServiceTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        var settleService = Substitute.For<ScaleOutSettleService>();
        scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        var matchService = Substitute.For<IMatchService>();
        var logDirectlyService = Substitute.For<LogService>();
        var xmlConvertService = Substitute.For<XmlConvertService>();
        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 15,
            EnvironmentName = "PRO",
            SiteDbServer = ["ABC"]
        };
        var appSettingsOptions = Options.Create(settings);

        service = new RefundMatchServiceImp(repository, scaleOutSettleService, appSettingsOptions, logDirectlyService, xmlConvertService, matchService);
        settleService.ValidateEventAndTicket(Arg.Any<EventTicketRequest>()).Returns(new ResultMessage { ErrorMessage = "" });
    }

    [Fact]
    public void Push_MatchTypeIsAutoRefundNormal_AlwaysReturnNotNull()
    {
        // Arrange
        var match = new Match { MatchId = 1 };

        // Act
        scaleOutSettleService.ValidateEventAndTicket(Arg.Any<EventTicketRequest>()).Returns(new ResultMessage());
        var result = service.Push(match);

        // Assert
        repository.Received().ExecuteAsync(Arg.Any<RefundPushCriteria>());
        Assert.NotNull(result);
    }
}