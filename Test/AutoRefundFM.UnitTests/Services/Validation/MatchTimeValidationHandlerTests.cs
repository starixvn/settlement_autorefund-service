using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class MatchTimeValidationHandlerTests
{
    private readonly MatchTimeValidationHandler sut = new();

    private static ValidationContext CreateContext(string homeValue, string awayValue)
        => new()
        {
            Session = new SpecialSession
            {
                MatchId = 123,
                HomeValue = homeValue,
                AwayValue = awayValue
            }
        };

    #region Success Cases

    [Fact]
    public async Task ProcessAsync_WithStartTimeLessThan45AndEndTimeLessOrEqualTo45_ReturnsSuccess()
    {
        // Arrange
        var context = CreateContext("30", "45");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_WithStartTime45OrMoreAndEndTimeGreaterThan45_ReturnsSuccess()
    {
        // Arrange  
        var context = CreateContext("45", "60");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Fact]
    public async Task ProcessAsync_WithStartTime90AndEndTime90_ReturnsSuccess()
    {
        // Arrange
        var context = CreateContext("90", "90");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    [Theory]
    [InlineData("30", "30")]
    [InlineData("44", "45")]
    [InlineData("0", "45")]
    [InlineData("45", "46")]
    [InlineData("60", "90")]
    public async Task ProcessAsync_WithValidTimeCombinations_ReturnsSuccess(string startTime, string endTime)
    {
        // Arrange
        var context = CreateContext(startTime, endTime);

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }

    #endregion

    #region Invalid Time Range Cases

    [Fact]
    public async Task ProcessAsync_WithStartTimeLessThan45AndEndTimeGreaterThan45_ReturnsError()
    {
        // Arrange
        var context = CreateContext("30", "60");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Invalid time range", result.ErrMsg);
        Assert.Contains("Start time (30) < 45 but end time (60) > 45", result.ErrMsg);
        Assert.True(result.MarkFail);
    }

    [Fact]
    public async Task ProcessAsync_WithStartTime0AndEndTime90_ReturnsError()
    {
        // Arrange
        var context = CreateContext("0", "90");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Start time (0) < 45 but end time (90) > 45", result.ErrMsg);
        Assert.True(result.MarkFail);
    }

    [Theory]
    [InlineData("10", "46")]
    [InlineData("44", "46")]
    [InlineData("1", "90")]
    [InlineData("20", "75")]
    public async Task ProcessAsync_WithInvalidTimeCombinations_ReturnsError(string startTime, string endTime)
    {
        // Arrange
        var context = CreateContext(startTime, endTime);

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Invalid time range", result.ErrMsg);
        Assert.True(result.MarkFail);
    }

    #endregion

    #region Invalid Input Format Cases

    [Fact]
    public async Task ProcessAsync_WithInvalidStartTimeFormat_ReturnsError()
    {
        // Arrange
        var context = CreateContext("abc", "30");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Invalid start time 'abc'", result.ErrMsg);
        Assert.True(result.MarkFail);
    }

    [Fact]
    public async Task ProcessAsync_WithInvalidEndTimeFormat_ReturnsError()
    {
        // Arrange
        var context = CreateContext("30", "xyz");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Invalid end time 'xyz'", result.ErrMsg);
        Assert.True(result.MarkFail);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyHomeValue_ReturnsError()
    {
        // Arrange
        var context = CreateContext("", "30");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Invalid start time ''", result.ErrMsg);
        Assert.True(result.MarkFail);
    }

    [Fact]
    public async Task ProcessAsync_WithEmptyAwayValue_ReturnsError()
    {
        // Arrange
        var context = CreateContext("30", "");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Invalid end time ''", result.ErrMsg);
        Assert.True(result.MarkFail);
    }

    [Theory]
    [InlineData("30.5", "45")]
    [InlineData("30", "45.0")]
    public async Task ProcessAsync_WithInvalidNumberFormats_ReturnsError(string startTime, string endTime)
    {
        // Arrange
        var context = CreateContext(startTime, endTime);

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.True(result.MarkFail);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ProcessAsync_WithNullSession_ThrowsException()
    {
        // Arrange
        var context = new ValidationContext { Session = null! };

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => sut.Handle(context));
    }

    [Fact]
    public async Task ProcessAsync_WithBoundaryValues_StartTime44EndTime46_ReturnsError()
    {
        // Arrange
        var context = CreateContext("44", "46");

        // Act
        var result = await sut.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidMatchTime, result.ErrCode);
        Assert.Contains("Start time (44) < 45 but end time (46) > 45", result.ErrMsg);
        Assert.True(result.MarkFail);
    }


    #endregion
}