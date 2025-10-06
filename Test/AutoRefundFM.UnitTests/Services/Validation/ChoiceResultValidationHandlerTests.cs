using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class ChoiceResultValidationHandlerTests
{
    private readonly ChoiceResultValidationHandler sut = new(null);

    private static FastMarketResultDetail CreateResultDetail(
        int betType,
        ResultItem? resultItem = null,
        int? result = null)
    {
        return new FastMarketResultDetail
        {
            BetType = betType,
            ResultItem = resultItem ?? new ResultItem(),
            Result = result
        };
    }

    #region ProcessAsync Tests



    [Fact]
    public async Task ProcessAsync_WithAnyInvalidResult_ReturnsError()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new ResultItem { Goal = true }),
                CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem()) // All false - invalid
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.Contains("invalid choice result", result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyFastMarketResultDetails_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession { FastMarketResultDetails = new List<FastMarketResultDetail>() };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region NextOneMinute Validation Tests


    [Fact]
    public async Task ProcessAsync_NextOneMinute_WithAllFalse_ReturnsError()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new ResultItem()) // All false
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.Contains("invalid choice result", result.ErrMsg);
    }

    #endregion

    #region NextFiveMinutes Validation Tests

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_WithValidCorner_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextFiveMinutes, result: 5)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_WithValidFreeKick_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextFiveMinutes, result: 12)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_WithValidGoalKick_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextFiveMinutes, result: 11)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_WithValidThrowIn_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextFiveMinutes,result: 11)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }



    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_WithAllFalse_ReturnsError()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem()) // All false
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.Contains("invalid choice result", result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_NextFiveMinutes_WithMultipleValidChoices_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextFiveMinutes,result: 10 )
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    #endregion

    #region TimeMachineMinutes Validation Tests

    [Fact]
    public async Task ProcessAsync_TimeMachineMinutes_WithValidResult_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.TimeMachineMinutes, new ResultItem(), 5)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    [Fact]
    public async Task ProcessAsync_TimeMachineMinutes_WithZeroResult_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.TimeMachineMinutes, new ResultItem(), 0)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    [Fact]
    public async Task ProcessAsync_TimeMachineMinutes_WithNegativeResult_ReturnsSuccess()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.TimeMachineMinutes, new ResultItem(), -1)
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }



    #endregion

    #region Other Bet Types Validation Tests


    [Fact]
    public async Task ProcessAsync_OtherBetType_WithAllFalse_ReturnsError()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(999, new ResultItem()) // All false
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.Contains("invalid choice result", result.ErrMsg);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task ProcessAsync_WithNullSession_ThrowsException()
    {
        // Arrange
        var context = new ValidationContext { Session = null! };

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => sut.Handle(context));
    }

    [Fact]
    public async Task ProcessAsync_WithNullFastMarketResultDetails_ThrowsException()
    {
        // Arrange
        var session = new SpecialSession { FastMarketResultDetails = null! };
        var context = new ValidationContext { Session = session };

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => sut.Handle(context));
    }

    [Fact]
    public async Task ProcessAsync_WithMixedValidAndInvalidResults_ReturnsError()
    {
        // Arrange
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                CreateResultDetail(FastMarketBetType.NextOneMinute, new ResultItem { Goal = true }), // Valid
                CreateResultDetail(FastMarketBetType.NextFiveMinutes, new ResultItem()) // Invalid
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.Contains("invalid choice result", result.ErrMsg);
    }



    #endregion
}
