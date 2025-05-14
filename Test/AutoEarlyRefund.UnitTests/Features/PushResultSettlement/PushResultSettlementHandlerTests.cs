using AutoEarlyRefund.Features.PushResultSettlement;
using AutoEarlyRefund.Models;
using AutoRefund.Core.Models;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AutoEarlyRefund.UnitTests.Features.PushResultSettlement;

public class PushResultSettlementHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly IOptions<AppSettingsKey> appSettingsKey;
    private readonly IPushResultSettlementHandler handler;
    private readonly CancellationToken cancellationToken;

    public PushResultSettlementHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        appSettingsKey = Substitute.For<IOptions<AppSettingsKey>>();
        handler = new PushResultSettlementHandler(repository, appSettingsKey);
        cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task HandleAsync_ShouldPushToAllConfiguredServers()
    {
        // Arrange
        var match = new Match
        {
            MatchId = 123,
            ResourcesXml = "<resources>test</resources>"
        };

        var servers = new List<string> { "Server1", "Server2", "Server3" };
        appSettingsKey.Value.Returns(new AppSettingsKey { SiteDbServer = servers });

        // Act
        await handler.HandleAsync(match, cancellationToken);

        // Assert
        await repository.Received(servers.Count).ExecuteAsync(
            Arg.Is<PushResultSettlementCriteria>(c =>
                c.MatchId == match.MatchId &&
                c.Action == "REFUND" &&
                c.IsAuto &&
                c.SelectedResources == match.ResourcesXml)
                );

    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var servers = new List<string> { "Server1" };
        var expectedException = new Exception("Repository error");

        appSettingsKey.Value.Returns(new AppSettingsKey { SiteDbServer = servers });
        repository.ExecuteAsync(Arg.Any<PushResultSettlementCriteria>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            handler.HandleAsync(match, cancellationToken));

        Assert.Same(expectedException, exception);
    }

    [Fact]
    public async Task HandleAsync_WithNoServers_ShouldCompleteSuccessfully()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        appSettingsKey.Value.Returns(new AppSettingsKey { SiteDbServer = new List<string>() });

        // Act
        await handler.HandleAsync(match, cancellationToken);

        // Assert
        await repository.DidNotReceive().ExecuteAsync(
                Arg.Any<PushResultSettlementCriteria>());
    }

    [Theory]
    [InlineData(null)]
    public void Constructor_WithNullDependencies_ShouldThrowArgumentNullException(object nullDependency)
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new PushResultSettlementHandler(
                nullDependency as IDynamicRepository,
                nullDependency as IOptions<AppSettingsKey>));
    }

    [Theory]
    [InlineData(null)]
    public async Task HandleAsync_WithNullMatch_ShouldThrowArgumentNullException(Match match)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(match, cancellationToken));
    }
}