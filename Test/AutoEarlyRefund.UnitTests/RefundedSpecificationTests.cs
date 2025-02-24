using AutoEarlyRefund;
using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Models;

public class RefundedSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_ReturnsTrue_WhenSettlementStatusIsRefunded()
    {
        // Arrange
        var resource = new Resource { SettlementStatusId = (short)SettleStatus.Refunded };
        var sourcesConditions = new SourcesConditions { Resource = resource };
        var specification = new RefundedSpecification();

        // Act
        var result = specification.IsSatisfiedBy(sourcesConditions);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_ReturnsFalse_WhenSettlementStatusIsNotRefunded()
    {
        // Arrange
        var resource = new Resource { SettlementStatusId = (short)SettleStatus.Calculated };
        var sourcesConditions = new SourcesConditions { Resource = resource };
        var specification = new RefundedSpecification();

        // Act
        var result = specification.IsSatisfiedBy(sourcesConditions);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var sourcesConditions = new SourcesConditions { Resource = null };
        var specification = new RefundedSpecification();

        // Act
        var result = specification.IsSatisfiedBy(sourcesConditions);

        // Assert
        Assert.False(result);
    }
}
