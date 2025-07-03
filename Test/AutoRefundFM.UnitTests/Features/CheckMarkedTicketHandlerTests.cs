using AutoRefund.Core.Models;
using AutoRefundFM.Features.CheckingMarkedTicket;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class CheckMarkedTicketHandlerTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly AppSettingsKey appSettingsKey = new()
    {
        SiteDbServer = new List<string> { "db1", "db2" }
    };
    private readonly IOptions<AppSettingsKey> options;

    public CheckMarkedTicketHandlerTests()
    {
        options = Options.Create(appSettingsKey);
    }

    [Fact]
    public async Task Handle_WhenAnyServerReturnsTrue_ReturnsTrue()
    {
        // Arrange
        repository.GetAsync<MarkedTickets>(Arg.Any<CheckingMarkedTicketCriteria>())
            .Returns(Task.FromResult(new MarkedTickets { IsExistMarkedTicket = false }),
                     Task.FromResult(new MarkedTickets { IsExistMarkedTicket = true }));
        var handler = new CheckMarkedTicketHandler(repository, options);

        // Act
        var result = await handler.Handle(1, "xml");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_WhenAllServersReturnFalse_ReturnsFalse()
    {
        // Arrange
        repository.GetAsync<MarkedTickets>(Arg.Any<CheckingMarkedTicketCriteria>())
            .Returns(Task.FromResult(new MarkedTickets { IsExistMarkedTicket = false }));
        var handler = new CheckMarkedTicketHandler(repository, options);

        // Act
        var result = await handler.Handle(1, "xml");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Handle_CreatesCriteriaWithCorrectValues()
    {
        // Arrange
        CheckingMarkedTicketCriteria capturedCriteria = null;
        repository.GetAsync<MarkedTickets>(Arg.Do<CheckingMarkedTicketCriteria>(c => capturedCriteria = c))
            .Returns(Task.FromResult(new MarkedTickets { IsExistMarkedTicket = false }));
        var handler = new CheckMarkedTicketHandler(repository, options);

        // Act
        await handler.Handle(42, "test-xml");

        // Assert
        Assert.NotNull(capturedCriteria);
        Assert.Equal(42, capturedCriteria.MatchId);
        Assert.Equal("test-xml", capturedCriteria.Results);
    }
}
