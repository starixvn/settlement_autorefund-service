using AutoEarlyRefund.Features.CheckMarkedTicket;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using AutoRefund.Core.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.CheckMarkedTicket;

public class CheckingMarkedTicketHandlerTests
{
    private readonly IValidateHandler nextHandler;
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly CheckingMarkedTicketHandler handler;

    public CheckingMarkedTicketHandlerTests()
    {
        nextHandler = Substitute.For<IValidateHandler>();
        repository = Substitute.For<IDynamicRepository>();
        appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();

        appSettingsKey.Value.Returns(new AppSettingsKey
        {
            SiteDbServer = new List<string> { "Server1", "Server2" }
        });

        handler = new CheckingMarkedTicketHandler(
            nextHandler,
            repository,
            appSettingsKey);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMarkedTicketsFound_ShouldCallNextHandler()
    {
        // Arrange
        var match = CreateSampleMatch();
        var expectedResult = new Result { ErrCode = 0, ErrMsg = "Success" };

        await repository.ExecuteAsync(Arg.Any<CheckingMarkedTicketCriteria>());

        nextHandler.HandleAsync(match).Returns(expectedResult);

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(expectedResult.ErrCode, result.ErrCode);
        Assert.Equal(expectedResult.ErrMsg, result.ErrMsg);

        await nextHandler.Received(1).HandleAsync(match);
    }



    private static Match CreateSampleMatch()
    {
        return new Match
        {
            MatchId = 123,
            ResultDetails = new List<ResultDetail>
            {
                new() { ResourceId = "Resource1" },
                new() { ResourceId = "Resource2" }
            }
        };
    }
}