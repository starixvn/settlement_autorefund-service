using AutoEarlyRefund.Features.CheckStatusRefundResults;
using AutoEarlyRefund.Features.InsOrUpdAutoRefundResult;
using AutoEarlyRefund.Features.WriteLog;
using AutoEarlyRefund.Models;
using AutoEarlyRefund.Services;
using AutoRefund.Core.Models;
using Fanex.Settlement.Converter.Services;
using NSubstitute;

namespace AutoEarlyRefund.UnitTests.Features.CheckStatusRefundResults;

public class CheckStatusRefundResultsHandlerTests : TestBase
{
    private readonly IValidateHandler nextHandler;
    private readonly IInsOrUpdAutoPayoutResultHandler insOrUpdAutoPayoutResult;
    private readonly CheckStatusRefundResultsHandler handler;

    public CheckStatusRefundResultsHandlerTests()
    {
        nextHandler = Substitute.For<IValidateHandler>();
        var xmlConvertService1 = Substitute.For<XmlConvertService>();
        insOrUpdAutoPayoutResult = Substitute.For<IInsOrUpdAutoPayoutResultHandler>();
        var writeLogService1 = Substitute.For<IWriteLogService>();

        handler = new CheckStatusRefundResultsHandler(
            insOrUpdAutoPayoutResult,
            xmlConvertService1,
            writeLogService1,
            nextHandler
            );
    }

    [Fact]
    public async Task HandleAsync_WhenNoSettledResults_ShouldCallNextHandler()
    {
        // Arrange
        var match = CreateSampleMatch([
            new ResultDetail { ResourceId = "1", ResultStatusId = 0 },
            new ResultDetail { ResourceId = "2", ResultStatusId = 1 }
        ]);
        var expectedResult = new Result { ErrCode = 0, ErrMsg = "Success" };
        nextHandler.HandleAsync(match).Returns(expectedResult);

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(expectedResult.ErrCode, result.ErrCode);
        Assert.Equal(expectedResult.ErrMsg, result.ErrMsg);
        await nextHandler.Received(1).HandleAsync(Arg.Is<Match>(m =>
            m.ResultDetails.Count == 2 &&
            m.ResultDetails.All(r => r.ResultStatusId != (int)SettleStatus.Calculated && r.ResultStatusId != (int)ResultStatus.AutoSettled)));
        await insOrUpdAutoPayoutResult.DidNotReceive().Handle(Arg.Any<Match>(), Arg.Any<int>());
    }

    [Fact]
    public async Task HandleAsync_WhenAllResultsSettled_ShouldThrowAppException()
    {
        // Arrange
        var match = CreateSampleMatch([
            new ResultDetail { ResourceId = "1", ResultStatusId =(int) ResultStatus.ManualSettled },
            new ResultDetail { ResourceId = "2", ResultStatusId =(int) ResultStatus.AutoSettled }
        ]);

        // Act & Assert
        await Assert.ThrowsAsync<AppException>(() => handler.HandleAsync(match));
        await nextHandler.DidNotReceive().HandleAsync(Arg.Any<Match>());
    }

    [Fact]
    public async Task HandleAsync_WithMixedResults_ShouldFilterSettledAndContinue()
    {
        // Arrange
        var match = CreateSampleMatch([
            new ResultDetail { ResourceId = "1", ResultStatusId = 0 },
            new ResultDetail { ResourceId = "2", ResultStatusId = (int)SettleStatus.Calculated },
            new ResultDetail { ResourceId = "3", ResultStatusId =(int) ResultStatus.AutoSettled },
            new ResultDetail { ResourceId = "4", ResultStatusId = 1 }
        ]);

        var expectedResult = new Result { ErrCode = 0, ErrMsg = "Success" };
        nextHandler.HandleAsync(Arg.Any<Match>()).Returns(expectedResult);

        // Act
        var result = await handler.HandleAsync(match);

        // Assert
        Assert.Equal(expectedResult.ErrCode, result.ErrCode);
        Assert.Equal(expectedResult.ErrMsg, result.ErrMsg);
    }

    private static Match CreateSampleMatch(List<ResultDetail> resultDetails)
    {
        return new Match
        {
            MatchId = 123,
            LeagueId = 456,
            SportType = 1,
            ChildMatchType = 2,
            ResultDetails = resultDetails
        };
    }
}