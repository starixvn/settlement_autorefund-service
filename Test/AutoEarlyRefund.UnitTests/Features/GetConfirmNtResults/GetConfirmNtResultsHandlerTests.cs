using AutoEarlyRefund.Features.GetConfirmNtResults;
using AutoEarlyRefund.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.MessageBridge.RefundEvents;
using MassTransit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AutoEarlyRefund.UnitTests.Features.GetConfirmNtResults;

public class GetConfirmNtResultsHandlerTests
{
    private readonly IDynamicRepository repository;
    private readonly IPublishEndpoint publishEndpoint;
    private readonly GetConfirmNtResultsHandler handler;

    public GetConfirmNtResultsHandlerTests()
    {
        repository = Substitute.For<IDynamicRepository>();
        publishEndpoint = Substitute.For<IPublishEndpoint>();
        handler = new GetConfirmNtResultsHandler(repository, publishEndpoint);
    }

    [Fact]
    public async Task Handle_WhenNoResults_ShouldNotPublishMessages()
    {
        // Arrange
        repository.FetchAsync<ResultDetail>(Arg.Any<GetConfirmNtResultsCriteria>())
            .Returns(new List<ResultDetail>());

        // Act
        await handler.Handle();

        // Assert
        await publishEndpoint.DidNotReceive().Publish(Arg.Any<NotTakenResultsConfirmed>());
    }

    [Fact]
    public async Task Handle_WithMultipleResults_ShouldGroupAndPublishMessages()
    {
        // Arrange
        var results = new List<ResultDetail>
        {
            CreateResultDetail(1, 100, 1, 1, "R1"),
            CreateResultDetail(1, 100, 1, 1, "R2"),
            CreateResultDetail(2, 200, 2, 2, "R3")
        };

        repository.FetchAsync<ResultDetail>(Arg.Any<GetConfirmNtResultsCriteria>())
            .Returns(results);

        // Act
        await handler.Handle();

        // Assert
        // Should publish 2 messages (one for each unique match)
        await publishEndpoint.Received(2).Publish(Arg.Any<NotTakenResultsConfirmed>());

        // Verify first match message
        await publishEndpoint.Received(1).Publish(Arg.Is<NotTakenResultsConfirmed>(msg =>
            msg.MatchId == 1 &&
            msg.ShortType == 1 &&
            msg.ChildMatchType == 1 &&
            msg.Resources.Count == 2 &&
            msg.Resources.Any(r => r.ResourceId == "R1") &&
            msg.Resources.Any(r => r.ResourceId == "R2")));

        // Verify second match message
        await publishEndpoint.Received(1).Publish(Arg.Is<NotTakenResultsConfirmed>(msg =>
            msg.MatchId == 2 &&
            msg.ShortType == 2 &&
            msg.ChildMatchType == 2 &&
            msg.Resources.Count == 1 &&
            msg.Resources.Any(r => r.ResourceId == "R3")));
    }

    [Fact]
    public async Task Handle_ShouldUseCriteriaCorrectly()
    {
        // Arrange
        repository.FetchAsync<ResultDetail>(Arg.Any<GetConfirmNtResultsCriteria>())
            .Returns(new List<ResultDetail>());

        // Act
        await handler.Handle();

        // Assert
        await repository.Received(1).FetchAsync<ResultDetail>(
            Arg.Is<GetConfirmNtResultsCriteria>(c =>
                c.GetSettingKey() == "NAP_Set_AutoRefund_ResourceCfm_ScheduleSel" &&
                c.IsValid()));
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Database error");
        repository.FetchAsync<ResultDetail>(Arg.Any<GetConfirmNtResultsCriteria>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => handler.Handle());
        Assert.Same(expectedException, exception);
        await publishEndpoint.DidNotReceive().Publish(Arg.Any<NotTakenResultsConfirmed>());
    }

    private static ResultDetail CreateResultDetail(int matchId, int leagueId, short sportType, short childMatchType, string resourceId)
    {
        return new ResultDetail
        {
            MatchId = matchId,
            LeagueId = leagueId,
            SportType = sportType,
            ChildMatchType = childMatchType,
            ResourceId = resourceId,
            HomeValue = "NT", // Adding this since it's used in ResourceItems
            ResultStatusId = (int)ResultStatus.Confirm // Adding this since it's used in ResourceItems
        };
    }
}