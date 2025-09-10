using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class ChoiceResultValidationHandlerTests
{

    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenAnyResultIsInvalid()
    {
        // Arrange
        var handler = new ChoiceResultValidationHandler();
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail
                {
                    BetType = FastMarketBetType.NextOneMinute,
                    ResultItem = new ResultItem() // All false
                },
                new FastMarketResultDetail
                {
                    BetType = FastMarketBetType.NextFiveMinutes,
                    ResultItem = new ResultItem { Penalty = true }
                }
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.InvalidChoiceResult, result.ErrCode);
        Assert.Contains("Invalid choice result", result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSuccess_WhenTimeMachineMinutes_HasResult()
    {
        // Arrange
        var handler = new ChoiceResultValidationHandler();
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail
                {
                    BetType = FastMarketBetType.TimeMachineMinutes,
                    Result = 5,
                    ResultItem = new ResultItem()
                }
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
    }
}
