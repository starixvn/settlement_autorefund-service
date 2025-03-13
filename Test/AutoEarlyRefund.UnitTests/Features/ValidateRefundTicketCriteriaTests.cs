using AutoEarlyRefund.Features.CheckEventAndTicket;

namespace AutoEarlyRefund.UnitTests.Features;

public class ValidateRefundTicketCriteriaTests
{
    [Fact]
    public void AutoType_ShouldBeInitializedTo7()
    {
        // Arrange
        var validateRefundTicketCriteria = new ValidateRefundTicketCriteria();

        // Act
        var autoType = validateRefundTicketCriteria.AutoType;

        // Assert
        Assert.Equal(7, autoType);
    }
}
