using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features;

public class PushFastMarketResultHandlerTests
{
    private readonly ScaleOutSettleService _scaleOutSettleService;
    private readonly IDynamicRepository _repository;
    private readonly IOptions<AppSettingsKey> _appSettingsKey;
    private readonly AutoRefundResultService _refundResultService;
    private readonly IUpdateWaterBreakHandler _updateWaterBreakHandler;
    private readonly SettlementLogService _logService;
    private readonly PushFastMarketResultHandler _handler;

    public PushFastMarketResultHandlerTests()
    {
        _scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        _repository = Substitute.For<IDynamicRepository>();
        _appSettingsKey = Options.Create(new AppSettingsKey
        {
            SiteDbServer = new List<string> { "Server1", "Server2" }
        });
        _refundResultService = Substitute.For<AutoRefundResultService>();
        _updateWaterBreakHandler = Substitute.For<IUpdateWaterBreakHandler>();
        _logService = Substitute.For<SettlementLogService>();

        _handler = new PushFastMarketResultHandler(
            _scaleOutSettleService,
            _repository,
            _appSettingsKey,
            _refundResultService,
            _updateWaterBreakHandler,
            _logService
        );
    }

    [Fact]
    public async Task Handle_ShouldUpdateMatchInfoAndExecuteCalculateForAllServers()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var criteria = new PushFastMarketResultCriteria
        {
            MatchId = 123,
            Results = "<results>test</results>"
        };
        var resourceXml = "<resources>test</resources>";

        // Act
        await _handler.Handle(match, criteria, resourceXml);

        // Assert
        await _scaleOutSettleService.Received(1)
            .UpdateMatchInfo(Arg.Is<UpdateMatchInfoRequest>(r =>
                r.Mode == Fanex.Settlement.ScaleOut.Constants.Mode.FastMarket));

        // Verify ExecuteCalculate was called for each server
        _repository.Received(2).Execute(Arg.Is<PushFastMarketResultCriteria>(c =>
            c.MatchId == match.MatchId));

        // Verify logs were written
        await _logService.Received(1)
            .WriteSettlementLog(Arg.Is<InsertSettlementLogCriteria>(c =>
                c.MatchId == match.MatchId &&
                c.Mode == Fanex.Settlement.ScaleOut.Constants.Mode.FastMarket));

        await _logService.Received(1)
            .WriteUserLogPreSettle(
                match.MatchId,
                Arg.Is<string>(s => s.Contains(match.MatchId.ToString())));

        // Verify water break update and results update
        await _updateWaterBreakHandler.Received(1).Handle(match);
        await _refundResultService.Received(1)
            .InsertOrUpdateResults(match.MatchId, 2, resourceXml);
    }

    [Fact]
    public async Task Handle_WhenUpdateMatchInfoFails_ShouldThrowException()
    {
        // Arrange
        var match = new Match { MatchId = 123 };
        var criteria = new PushFastMarketResultCriteria();
        var resourceXml = "<resources>test</resources>";

        _scaleOutSettleService
            .UpdateMatchInfo(Arg.Any<UpdateMatchInfoRequest>())
            .Returns(Task.FromException(new Exception("Update failed")));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(match, criteria, resourceXml));
    }

    [Fact]
    public async Task Handle_WithEmptyServerList_ShouldNotExecuteCalculate()
    {
        // Arrange
        var emptyServerOptions = Options.Create(new AppSettingsKey
        {
            SiteDbServer = new List<string>()
        });

        var handler = new PushFastMarketResultHandler(
            _scaleOutSettleService,
            _repository,
            emptyServerOptions,
            _refundResultService,
            _updateWaterBreakHandler,
            _logService
        );

        var match = new Match { MatchId = 123 };
        var criteria = new PushFastMarketResultCriteria();
        var resourceXml = "<resources>test</resources>";

        // Act
        await handler.Handle(match, criteria, resourceXml);

        // Assert
        _repository.DidNotReceive().Execute(Arg.Any<PushFastMarketResultCriteria>());
    }
}