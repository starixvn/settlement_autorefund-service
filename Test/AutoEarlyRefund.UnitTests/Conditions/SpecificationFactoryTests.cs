using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.UnitTests.Conditions;

public class SpecificationFactoryTests
{
    [Fact]
    public void CreateSpecification_RefundExtraMatch_ReturnsExtraMatchRefundedSpecification()
    {
        // Arrange
        var condition = new SourcesConditions
        {
            SourceAction = "Refund",
            SourceMatchType = 3,
            SourceChildMatchType = 201,
            Conditions = "0"
        };

        // Act
        var specification = SpecificationFactory.CreateSpecification(condition);

        // Assert
        Assert.IsType<ExtraMatchRefundedSpecification>(specification);
    }

    [Fact]
    public void CreateSpecification_Refund_ReturnsRefundedSpecification()
    {
        // Arrange
        var condition = new SourcesConditions
        {
            SourceAction = "Refund"
        };

        // Act
        var specification = SpecificationFactory.CreateSpecification(condition);

        // Assert
        Assert.IsType<RefundedSpecification>(specification);
    }

    [Fact]
    public void CreateSpecification_CalParentMatch_ReturnsParentMatchCalculatedWinLostSpecification()
    {
        // Arrange
        var condition = new SourcesConditions
        {
            SourceAction = "Cal",
            SourceMatchType = 2,
            SourceChildMatchType = 0,
            Conditions = "1"
        };

        // Act
        var specification = SpecificationFactory.CreateSpecification(condition);

        // Assert
        Assert.IsType<ParentMatchCalculatedWinLostSpecification>(specification);
    }

    [Fact]
    public void CreateSpecification_CalExtraMatch_ReturnsExtraMatchCalculatedWinLostSpecification()
    {
        // Arrange
        var condition = new SourcesConditions
        {
            SourceAction = "Cal",
            SourceMatchType = 3,
            SourceChildMatchType = 201,
            Conditions = "1"
        };

        // Act
        var specification = SpecificationFactory.CreateSpecification(condition);

        // Assert
        Assert.IsType<ExtraMatchCalculatedWinLostSpecification>(specification);
    }

    [Fact]
    public void CreateSpecification_Cal_ReturnsCalculatedSpecification()
    {
        // Arrange
        var condition = new SourcesConditions
        {
            SourceAction = "Cal",
            SourceMatchType = 1,
            SourceChildMatchType = 202,
            Conditions = "0"
        };

        // Act
        var specification = SpecificationFactory.CreateSpecification(condition);

        // Assert
        Assert.IsType<CalculatedSpecification>(specification);
    }

    [Fact]
    public void CreateSpecification_CfnRefund_ReturnsConfirmedRefundNTSpecification()
    {
        // Arrange
        var condition = new SourcesConditions
        {
            SourceAction = "CfnRefund",
            SourceMatchType = 1,
            SourceChildMatchType = 202,
            Conditions = "3"
        };

        // Act
        var specification = SpecificationFactory.CreateSpecification(condition);

        // Assert
        Assert.IsType<ConfirmedRefundNTSpecification>(specification);
    }

    [Fact]
    public void CreateSpecification_Default_ReturnsDefaultSpecification()
    {
        // Arrange
        var condition = new SourcesConditions
        {
            SourceAction = "Unknown"
        };

        // Act
        var specification = SpecificationFactory.CreateSpecification(condition);

        // Assert
        Assert.IsType<DefaultSpecification>(specification);
    }
}
