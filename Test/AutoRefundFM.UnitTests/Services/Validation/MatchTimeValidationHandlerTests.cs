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

    #endregion

    #region Invalid Time Range Cases

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
        Assert.Contains("Start time (44) < 45 but end time (46) >= 45", result.ErrMsg);
        Assert.True(result.MarkFail);
    }


    #endregion
}