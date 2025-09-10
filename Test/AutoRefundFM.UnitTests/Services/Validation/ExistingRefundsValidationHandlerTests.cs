using AutoRefund.Core.Models;
using AutoRefund.Core.Services;
using AutoRefundFM.Models;
using AutoRefundFM.Services.Validation;
using NSubstitute;

namespace AutoRefundFM.UnitTests.Services.Validation;

public class ExistingRefundsValidationHandlerTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenExistingRefundsFound()
    {
        // Arrange
        var refundResultService = Substitute.For<AutoRefundResultService>();
        var handler = new ExistingRefundsValidationHandler(refundResultService);
        var fmResult = new FastMarketResultDetail { BetType = 1, Position = 2 };
        var session = new SpecialSession
        {
            MatchId = 10,
            FastMarketResultDetails = new List<FastMarketResultDetail> { fmResult }
        };
        var refundResults = new List<AutoRefundResult>
        {
            new AutoRefundResult { ResourceId = "1", OtherInfo = "2" }
        };
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>()).Returns(refundResults);
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(FastMarketErrorCodes.ExistingRefundsFound, result.ErrCode);
        Assert.Contains("Existing refunds found", result.ErrMsg);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSuccess_WhenNoExistingRefunds()
    {
        // Arrange
        var refundResultService = Substitute.For<AutoRefundResultService>();
        var handler = new ExistingRefundsValidationHandler(refundResultService);
        var fmResult = new FastMarketResultDetail { BetType = 1, Position = 2 };
        var session = new SpecialSession
        {
            MatchId = 10,
            FastMarketResultDetails = new List<FastMarketResultDetail> { fmResult }
        };
        var refundResults = new List<AutoRefundResult>();
        refundResultService.GetResults(Arg.Any<List<GetRefundResultsItem>>()).Returns(refundResults);
        var context = new ValidationContext { Session = session };

        // Act
        var result = await handler.Handle(context);

        // Assert
        Assert.Equal(0, result.ErrCode);
        Assert.True(string.IsNullOrEmpty(result.ErrMsg));
    }
}
