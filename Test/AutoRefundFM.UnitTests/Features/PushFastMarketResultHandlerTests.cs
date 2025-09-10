using AutoRefund.Core.Criteria;
using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Features.PushFastMarketResult;
using AutoRefundFM.Features.UpdateWaterBreak;
using AutoRefundFM.Models;
using AutoRefundFM.Services;
using Fanex.Data.Repository;
using Fanex.Settlement.ScaleOut.Request;
using Fanex.Settlement.ScaleOut.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Features
{
    public class PushFastMarketResultHandlerTests
    {
        private readonly ScaleOutSettleService scaleOutSettleService = Substitute.For<ScaleOutSettleService>();
        private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
        private readonly IOptions<AppSettingsKey> options = Substitute.For<IOptions<AppSettingsKey>>();
        private readonly IUpdateWaterBreakHandler updateWaterBreakHandler = Substitute.For<IUpdateWaterBreakHandler>();
        private readonly SettlementLogService logService = Substitute.For<SettlementLogService>();
        private readonly IDataTransformationService dataTransformationService = Substitute.For<IDataTransformationService>();
        private readonly AppSettingsKey appSettingsKey = new() { SiteDbServer = new List<string> { "server1", "server2" } };

        private readonly PushFastMarketResultHandler sut;

        public PushFastMarketResultHandlerTests()
        {
            options.Value.Returns(appSettingsKey);
            sut = new PushFastMarketResultHandler(
                scaleOutSettleService,
                repository,
                options,
                updateWaterBreakHandler,
                logService,
                dataTransformationService);
        }

        [Fact]
        public async Task Handle_CallsUpdateMatchInfo_WithCorrectRequest()
        {
            // Arrange
            var session = new SpecialSession { MatchId = 1 };
            var criteria = new PushFastMarketResultCriteria { Results = "<results/>" };

            // Act
            await sut.Handle(session, criteria);

            // Assert
            await scaleOutSettleService.Received(1).UpdateMatchInfo(Arg.Is<UpdateMatchInfoRequest>(r =>
                r.Mode == Fanex.Settlement.ScaleOut.Constants.Mode.FastMarket));
        }

        [Fact]
        public async Task Handle_WritesPreSettlementAndSettlementLogs()
        {
            // Arrange
            var session = new SpecialSession
            {
                MatchId = 1,
                FastMarketResultDetails = new List<FastMarketResultDetail>()
            };
            var criteria = new PushFastMarketResultCriteria { Results = "<results/>" };
            dataTransformationService.CreateResourcesFromFmResult(Arg.Any<List<FastMarketResultDetail>>())
                .Returns((new List<ResourceItem>(), "<resources/>"));

            // Act
            await sut.Handle(session, criteria);

            // Assert
            await logService.Received(1).WriteUserLogPreSettle(1, Arg.Any<string>());
            await logService.Received(1).WriteSettlementLog(Arg.Is<InsertSettlementLogCriteria>(c =>
                c.MatchId == 1 &&
                c.Action == "REFUND" &&
                c.Mode == Fanex.Settlement.ScaleOut.Constants.Mode.FastMarket &&
                c.IsAuto));
        }

        [Fact]
        public async Task Handle_ExecutesCalculate_ForEachServer()
        {
            // Arrange
            var session = new SpecialSession { MatchId = 1 };
            var criteria = new PushFastMarketResultCriteria { Results = "<results/>" };

            // Act
            await sut.Handle(session, criteria);

            // Assert
            await repository.Received(2).ExecuteAsync(Arg.Is<PushFastMarketResultCriteria>(c =>
                c.DbServer == "server1" || c.DbServer == "server2"));
        }

        [Fact]
        public async Task Handle_UpdatesWaterBreak_WithStatusId2()
        {
            // Arrange
            var session = new SpecialSession { MatchId = 1 };
            var criteria = new PushFastMarketResultCriteria { Results = "<results/>" };

            // Act
            await sut.Handle(session, criteria);

            // Assert
            await updateWaterBreakHandler.Received(1).Handle(1, Arg.Is<List<SpecialSession>>(l => l.Contains(session)), 2);
        }
    }
}
