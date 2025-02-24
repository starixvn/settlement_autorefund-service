using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Models;

public class ConfirmedRefundSdtSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_ResourceIsNull_ReturnsFalse()
    {
        // Arrange
        var specification = new ConfirmedRefundSdtSpecification();
        var sourcesConditions = new SourcesConditions { Resource = null };

        // Act
        var result = specification.IsSatisfiedBy(sourcesConditions);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSatisfiedBy_ResourceIsConfirmedRefundSdt_ReturnsTrue()
    {
        // Arrange
        var specification = new ConfirmedRefundSdtSpecification();
        var resource = new Resource { ResultSetting = 4 };
        var sourcesConditions = new SourcesConditions { Resource = resource };

        // Act
        var result = specification.IsSatisfiedBy(sourcesConditions);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSatisfiedBy_ResourceIsNotConfirmedRefundSdt_ReturnsFalse()
    {
        // Arrange
        var specification = new ConfirmedRefundSdtSpecification();
        var resource = new Resource { ResultSetting = 4 };
        var sourcesConditions = new SourcesConditions { Resource = resource };

        // Act
        var result = specification.IsSatisfiedBy(sourcesConditions);

        // Assert
        Assert.False(result);
    }
}
