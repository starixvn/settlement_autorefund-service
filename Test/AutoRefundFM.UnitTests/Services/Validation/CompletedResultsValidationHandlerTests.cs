using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class CompletedResultsValidationHandlerTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenAnyResultIsCompleted()
    {
        // Arrange
        var handler = new CompletedResultsValidationHandler();
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail { ResultStatus = Status.CompletedManualResult },
                new FastMarketResultDetail { ResultStatus = 0 }
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.NoCompletedResults, result.ErrCode);
        Assert.Contains("Auto Refund failed", result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSuccess_WhenNoResultIsCompleted()
    {
        // Arrange
        var handler = new CompletedResultsValidationHandler();
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail { ResultStatus = 0 },
                new FastMarketResultDetail { ResultStatus = 1 }
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }

    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenAnyResultIsAutoCompleted()
    {
        // Arrange
        var handler = new CompletedResultsValidationHandler();
        var session = new SpecialSession
        {
            FastMarketResultDetails = new List<FastMarketResultDetail>
            {
                new FastMarketResultDetail { ResultStatus = Status.CompletedAutoResult },
                new FastMarketResultDetail { ResultStatus = 2 }
            }
        };
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.NoCompletedResults, result.ErrCode);
        Assert.Contains("Auto Refund failed", result.ErrMsg);
    }
}
