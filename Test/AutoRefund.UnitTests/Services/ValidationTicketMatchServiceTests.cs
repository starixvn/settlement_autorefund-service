using AutoRefund.Models;
using AutoRefund.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AutoRefund.UnitTests.Services;

public class ValidationTicketMatchServiceTests
{
    private readonly IValidationTicketMatchService service;
    private readonly IDynamicRepository repository;

    public ValidationTicketMatchServiceTests()
    {
        var settings = new AppSettingsKey()
        {
            RetryInterval = 1000,
            RetryCount = 15,
            EnvironmentName = "PRO",
            SiteDbServer = new List<string> { "ABC" }
        };
        var appSettingsOptions = Options.Create(settings);
        repository = Substitute.For<IDynamicRepository>();

        service = new ValidationTicketServiceImp(appSettingsOptions, repository);
    }


    [Fact]
    public async void VerifyTicketMatch_PassAllMatchChecking_DoesNotThrowsSettledTicketException()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 1,
            TypeId = 1,

        };

        // Assert
        var exception = await Record.ExceptionAsync(() => service.VerifyTicketMatch(match));
        Assert.Null(exception);
    }
}