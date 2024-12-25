using AutoRefund.Criteria;
using AutoRefund.Exceptions;
using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class ValidationTicketMatchServiceTests
{
    private readonly IDynamicRepository repository;
    private readonly ValidationTicketServiceImp service;

    public ValidationTicketMatchServiceTests()
    {
        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 15,
            EnvironmentName = "PRO",
            SiteDbServer = ["ABC"]
        };
        var appSettingsOptions = Options.Create(settings);
        repository = Substitute.For<IDynamicRepository>();

        service = new ValidationTicketServiceImp(appSettingsOptions, repository);
    }

    [Fact]
    public async Task VerifyTicketMatch_PassAllMatchChecking_DoesNotThrowsSettledTicketException()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            TypeId = 1,
        };

        // Assert
        var exception = await Record.ExceptionAsync(async () => await service.VerifyTicketMatch(match));
        Assert.Null(exception);
    }

    [Fact]
    public async Task VerifyTicketMatch_ExistedMarkedTicket_ThrowException()
    {
        var match = new Match
        {
            MatchId = 1,
            TypeId = 1,
        };

        var matchTicketInfo = new MatchTicketInfo
        {
            IsExistMarkedTicket = true
        };
        repository.GetAsync<MatchTicketInfo>(Arg.Any<MatchTicketInfoCriteria>()).Returns(matchTicketInfo);

        await Assert.ThrowsAsync<MarkedTicketException>(async () => await service.VerifyTicketMatch(match));
    }

    [Fact]
    public async Task VerifyTicketMatch_ExistedIsExistSettledTicket_ThrowException()
    {
        var match = new Match
        {
            MatchId = 1,
            TypeId = 1,
        };

        var matchTicketInfo = new MatchTicketInfo
        {
            IsExistSettledTicket = true
        };
        repository.GetAsync<MatchTicketInfo>(Arg.Any<MatchTicketInfoCriteria>()).Returns(matchTicketInfo);

        await Assert.ThrowsAsync<SettledTicketException>(async () => await service.VerifyTicketMatch(match));
    }
}