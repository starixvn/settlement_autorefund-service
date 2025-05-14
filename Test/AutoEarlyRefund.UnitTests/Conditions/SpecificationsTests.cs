using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.UnitTests.Conditions;

public class SpecificationsTests
{
    [Fact]
    public void DefaultSpecification_IsSatisfiedBy_ReturnsTrue()
    {
        var spec = new DefaultSpecification();
        var entity = new SourcesConditions();
        Assert.False(spec.IsSatisfiedBy(entity));
    }

    [Fact]
    public void RefundedSpecification_IsSatisfiedBy_ReturnsTrue_WhenRefunded()
    {
        var spec = new RefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)SettleStatus.Refunded }
        };
        Assert.True(spec.IsSatisfiedBy(entity));
    }

    [Fact]
    public void CalculatedSpecification_IsSatisfiedBy_ReturnsTrue_WhenCalculated()
    {
        var spec = new CalculatedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)SettleStatus.Calculated }
        };
        Assert.True(spec.IsSatisfiedBy(entity));
    }


    [Fact]
    public void ConfirmedRefundNTSpecification_IsSatisfiedBy_ReturnsTrue_WhenConfirmedRefundNT()
    {
        var spec = new ConfirmedRefundNTSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                ResultStatusId = (int)ResultStatus.Confirm,
                HomeValue = "NT"
            }
        };
        Assert.True(spec.IsSatisfiedBy(entity));
    }

    [Fact]
    public void ParentMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsTrue_WhenCalculatedAndValuesNotEqual()
    {
        var spec = new ParentMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Calculated,
                HomeValue = "A",
                AwayValue = "B"
            }
        };
        Assert.True(spec.IsSatisfiedBy(entity));
    }

    [Fact]
    public void ExtraMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsTrue_WhenCalculatedAndValuesNotEqual()
    {
        var spec = new ExtraMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Calculated,
                HomeValue = "A",
                AwayValue = "B"
            }
        };
        Assert.True(spec.IsSatisfiedBy(entity));
    }

    [Fact]
    public void ExtraMatchRefundedSpecification_IsSatisfiedBy_ReturnsTrue_WhenRefundedAndSourceChildMatchTypeIs201()
    {
        var spec = new ExtraMatchRefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Refunded,
                SourceChildMatchType = 201
            }
        };
        Assert.True(spec.IsSatisfiedBy(entity));
    }
}
