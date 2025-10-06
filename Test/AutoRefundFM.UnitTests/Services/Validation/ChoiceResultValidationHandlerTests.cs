using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;
using Microsoft.Extensions.Options;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class ChoiceResultValidationHandlerTests
{
    private static ChoiceResultValidationHandler CreateHandler(int retryProcessScoreMicroseconds = 2_000_000)
        => new(Options.Create(new AppSettingsKey { RetryProcessScore = retryProcessScoreMicroseconds }));

    private static FastMarketResultDetail CreateResultDetail(int betType, ResultItem? resultItem = null, int? result = null)
        => new()
        {
            BetType = betType,
            ResultItem = resultItem ?? new ResultItem(),
            Result = result
        };

    private static ValidationContext CreateContext(params FastMarketResultDetail[] details)
        => new()
        {
            Session = new SpecialSession
            {
                FastMarketResultDetails = details.ToList(),
                FirstProcess = DateTimeOffset.Now // default: fresh
            }
        };

    #region Success / No Error

    [Fact]
    public async Task ProcessAsync_WhenAllValid_ReturnsSuccess()
    {
        // Arrange (one valid for each branch)
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextOneMinute, new ResultItem { Goal = true }, 2),
            CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem { Penalty = true }, 3),
            CreateResultDetail(FastMarketBetType.TimeMachineMinutes, new ResultItem(), 1),
            CreateResultDetail(999, new ResultItem { Bookings = true }, 2) // falls into "other"
        );

        // Act
        var result = await handler.Handle(ctx);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_EmptyFastMarketResultDetails_ReturnsSuccess()
    {
        var handler = CreateHandler();
        var ctx = new ValidationContext
        {
            Session = new SpecialSession
            {
                FastMarketResultDetails = new List<FastMarketResultDetail>(),
                FirstProcess = DateTimeOffset.Now
            }
        };

        var result = await handler.Handle(ctx);

        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region NextOneMinute

    [Fact]
    public async Task ProcessAsync_NextOneMinute_AllFalse_IsError()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextOneMinute, new ResultItem()) // all false
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_NextOneMinute_OneFlagTrue_IsSuccess()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextOneMinute, new ResultItem { ThrowIn = true }, 2)
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region NextFiveMinutes

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_AllFalse_IsError()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem()) // invalid
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_PenaltyTrue_IsSuccess()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextFiveMinutes, null, 2)
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region TimeMachineMinutes

    [Fact]
    public async Task ProcessAsync_TimeMachineMinutes_WithNegativeValue_IsSuccess()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.TimeMachineMinutes, new ResultItem(), -5)
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region Other Bet Types

    [Fact]
    public async Task ProcessAsync_Other_AllFalse_IsError()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(888, new ResultItem()) // falls into other
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_Other_BookingsTrue_IsSuccess()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(888, new ResultItem { Bookings = true }, 4)
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region Mixed / Aggregation

    [Fact]
    public async Task ProcessAsync_MixedContainsOneInvalid_ReturnsError()
    {
        var handler = CreateHandler();
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextOneMinute, new ResultItem { Goal = true }),
            CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem()) // invalid
        );

        var result = await handler.Handle(ctx);

        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
    }

    #endregion

    #region MarkFail Timing Logic

    [Fact]
    public async Task ProcessAsync_Invalid_NotExpired_MarkFailFalse()
    {
        // RetryProcessScore = 2_000_000 microseconds (2s). FirstProcess only 1s ago.
        var handler = CreateHandler(2_000_000);
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem()) // invalid
        );
        ctx.Session.FirstProcess = DateTimeOffset.Now.AddSeconds(-1);

        var result = await handler.Handle(ctx);

        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.False(result.MarkFail);
    }

    [Fact]
    public async Task ProcessAsync_Invalid_Expired_MarkFailTrue()
    {
        // RetryProcessScore = 2_000_000 microseconds (2s). FirstProcess 5s ago.
        var handler = CreateHandler(2_000_000);
        var ctx = CreateContext(
            CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem()) // invalid
        );
        ctx.Session.FirstProcess = DateTimeOffset.Now.AddSeconds(-5);

        var result = await handler.Handle(ctx);

        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.True(result.MarkFail);
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task ProcessAsync_NullSession_Throws()
    {
        var handler = CreateHandler();
        var ctx = new ValidationContext { Session = null! };
        await Assert.ThrowsAsync<NullReferenceException>(() => handler.Handle(ctx));
    }

    [Fact]
    public async Task ProcessAsync_NullFastMarketResultDetails_Throws()
    {
        var handler = CreateHandler();
        var ctx = new ValidationContext
        {
            Session = new SpecialSession
            {
                FastMarketResultDetails = null!,
                FirstProcess = DateTimeOffset.Now
            }
        };
        await Assert.ThrowsAsync<NullReferenceException>(() => handler.Handle(ctx));
    }

    #endregion
}
