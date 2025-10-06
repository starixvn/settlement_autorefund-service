using AutoEarlyRefund.Conditions;
using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.UnitTests.Conditions;

public class SpecificationsTests
{
    #region DefaultSpecification Tests

    [Fact]
    public void DefaultSpecification_IsSatisfiedBy_ReturnsFalse()
    {
        // Arrange
        var spec = new DefaultSpecification();
        var entity = new SourcesConditions();

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DefaultSpecification_IsSatisfiedBy_ReturnsFalse_WithNullResource()
    {
        // Arrange
        var spec = new DefaultSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DefaultSpecification_IsSatisfiedBy_ReturnsFalse_WithAnyResource()
    {
        // Arrange
        var spec = new DefaultSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)SettleStatus.Refunded }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region RefundedSpecification Tests

    [Fact]
    public void RefundedSpecification_IsSatisfiedBy_ReturnsTrue_WhenRefunded()
    {
        // Arrange
        var spec = new RefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)SettleStatus.Refunded }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RefundedSpecification_IsSatisfiedBy_ReturnsFalse_WhenNotRefunded()
    {
        // Arrange
        var spec = new RefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)SettleStatus.Calculated }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RefundedSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new RefundedSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(SettleStatus.Running)]
    [InlineData(SettleStatus.RefundedHt)]
    [InlineData(SettleStatus.RefundedFt)]
    [InlineData(SettleStatus.Calculated)]
    public void RefundedSpecification_IsSatisfiedBy_ReturnsFalse_ForNonRefundedStatuses(SettleStatus status)
    {
        // Arrange
        var spec = new RefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)status }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region CalculatedSpecification Tests

    [Fact]
    public void CalculatedSpecification_IsSatisfiedBy_ReturnsTrue_WhenCalculated()
    {
        // Arrange
        var spec = new CalculatedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)SettleStatus.Calculated }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CalculatedSpecification_IsSatisfiedBy_ReturnsFalse_WhenNotCalculated()
    {
        // Arrange
        var spec = new CalculatedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)SettleStatus.Refunded }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CalculatedSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new CalculatedSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(SettleStatus.Running)]
    [InlineData(SettleStatus.Refunded)]
    [InlineData(SettleStatus.RefundedHt)]
    [InlineData(SettleStatus.RefundedFt)]
    public void CalculatedSpecification_IsSatisfiedBy_ReturnsFalse_ForNonCalculatedStatuses(SettleStatus status)
    {
        // Arrange
        var spec = new CalculatedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { SettlementStatusId = (short)status }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ConfirmedRefundSdtSpecification Tests

    [Fact]
    public void ConfirmedRefundSdtSpecification_IsSatisfiedBy_ReturnsTrue_WhenConfirmedRefundSdt()
    {
        // Arrange
        var spec = new ConfirmedRefundSdtSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { ResultSetting = 4 } // Bit 2 set (1 << 2)
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ConfirmedRefundSdtSpecification_IsSatisfiedBy_ReturnsFalse_WhenNotConfirmedRefundSdt()
    {
        // Arrange
        var spec = new ConfirmedRefundSdtSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { ResultSetting = 0 }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConfirmedRefundSdtSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new ConfirmedRefundSdtSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ConfirmedRefundSdtSpecification_IsSatisfiedBy_ReturnsFalse_ForNonSdtSettings(byte resultSetting)
    {
        // Arrange
        var spec = new ConfirmedRefundSdtSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { ResultSetting = resultSetting }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ConfirmedRefundSpecification Tests

    [Fact]
    public void ConfirmedRefundSpecification_IsSatisfiedBy_ReturnsTrue_WhenConfirmedRefund()
    {
        // Arrange
        var spec = new ConfirmedRefundSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { ResultSetting = 2 } // Bit 1 set (1 << 1)
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ConfirmedRefundSpecification_IsSatisfiedBy_ReturnsFalse_WhenNotConfirmedRefund()
    {
        // Arrange
        var spec = new ConfirmedRefundSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { ResultSetting = 0 }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConfirmedRefundSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new ConfirmedRefundSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    public void ConfirmedRefundSpecification_IsSatisfiedBy_ReturnsFalse_ForNonRefundSettings(byte resultSetting)
    {
        // Arrange
        var spec = new ConfirmedRefundSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource { ResultSetting = resultSetting }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ConfirmedRefundNTSpecification Tests

    [Fact]
    public void ConfirmedRefundNTSpecification_IsSatisfiedBy_ReturnsTrue_WhenConfirmedRefundNT()
    {
        // Arrange
        var spec = new ConfirmedRefundNTSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                ResultStatusId = (short)ResultStatus.Confirm,
                HomeValue = "NT"
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ConfirmedRefundNTSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new ConfirmedRefundNTSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConfirmedRefundNTSpecification_IsSatisfiedBy_ReturnsFalse_WhenNotConfirmStatus()
    {
        // Arrange
        var spec = new ConfirmedRefundNTSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                ResultStatusId = (short)ResultStatus.AutoSettled,
                HomeValue = "NT"
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConfirmedRefundNTSpecification_IsSatisfiedBy_ReturnsFalse_WhenHomeValueNotNT()
    {
        // Arrange
        var spec = new ConfirmedRefundNTSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                ResultStatusId = (short)ResultStatus.Confirm,
                HomeValue = "A"
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(ResultStatus.AutoSettled, "NT")]
    [InlineData(ResultStatus.UnCal, "NT")]
    [InlineData(ResultStatus.ManualSettled, "NT")]
    [InlineData(ResultStatus.Confirm, "A")]
    [InlineData(ResultStatus.Confirm, "B")]
    [InlineData(ResultStatus.Confirm, "")]
    public void ConfirmedRefundNTSpecification_IsSatisfiedBy_ReturnsFalse_ForInvalidConditions(ResultStatus status, string homeValue)
    {
        // Arrange
        var spec = new ConfirmedRefundNTSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                ResultStatusId = (short)status,
                HomeValue = homeValue
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ParentMatchCalculatedWinLostSpecification Tests

    [Fact]
    public void ParentMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsTrue_WhenCalculatedAndValuesNotEqual()
    {
        // Arrange
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

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ParentMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsFalse_WhenNotCalculated()
    {
        // Arrange
        var spec = new ParentMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Refunded,
                HomeValue = "A",
                AwayValue = "B"
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ParentMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsFalse_WhenValuesEqual()
    {
        // Arrange
        var spec = new ParentMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Calculated,
                HomeValue = "A",
                AwayValue = "A"
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ParentMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new ParentMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("A", "B")]
    [InlineData("1", "2")]
    [InlineData("Home", "Away")]
    [InlineData("Win", "Lose")]
    public void ParentMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsTrue_ForDifferentValues(string homeValue, string awayValue)
    {
        // Arrange
        var spec = new ParentMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Calculated,
                HomeValue = homeValue,
                AwayValue = awayValue
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("A", "A")]
    [InlineData("1", "1")]
    [InlineData("", "")]
    [InlineData("Same", "Same")]
    public void ParentMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsFalse_ForEqualValues(string homeValue, string awayValue)
    {
        // Arrange
        var spec = new ParentMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Calculated,
                HomeValue = homeValue,
                AwayValue = awayValue
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ExtraMatchCalculatedWinLostSpecification Tests

    [Fact]
    public void ExtraMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsTrue_WhenCalculatedAndValuesNotEqual()
    {
        // Arrange
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

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ExtraMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsFalse_WhenNotCalculated()
    {
        // Arrange
        var spec = new ExtraMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Refunded,
                HomeValue = "A",
                AwayValue = "B"
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtraMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsFalse_WhenValuesEqual()
    {
        // Arrange
        var spec = new ExtraMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Calculated,
                HomeValue = "A",
                AwayValue = "A"
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtraMatchCalculatedWinLostSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new ExtraMatchCalculatedWinLostSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ExtraMatchRefundedSpecification Tests

    [Fact]
    public void ExtraMatchRefundedSpecification_IsSatisfiedBy_ReturnsTrue_WhenRefundedAndSourceChildMatchTypeIs201()
    {
        // Arrange
        var spec = new ExtraMatchRefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Refunded,
                SourceChildMatchType = 201
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ExtraMatchRefundedSpecification_IsSatisfiedBy_ReturnsTrue_WhenRefundedFtAndSourceChildMatchTypeIs201()
    {
        // Arrange
        var spec = new ExtraMatchRefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.RefundedFt,
                SourceChildMatchType = 201
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ExtraMatchRefundedSpecification_IsSatisfiedBy_ReturnsFalse_WhenRefundedButSourceChildMatchTypeNot201()
    {
        // Arrange
        var spec = new ExtraMatchRefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Refunded,
                SourceChildMatchType = 200
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtraMatchRefundedSpecification_IsSatisfiedBy_ReturnsFalse_WhenSourceChildMatchTypeIs201ButNotRefunded()
    {
        // Arrange
        var spec = new ExtraMatchRefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)SettleStatus.Calculated,
                SourceChildMatchType = 201
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtraMatchRefundedSpecification_IsSatisfiedBy_ReturnsFalse_WhenResourceIsNull()
    {
        // Arrange
        var spec = new ExtraMatchRefundedSpecification();
        var entity = new SourcesConditions { Resource = null };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(SettleStatus.Running, 201)]
    [InlineData(SettleStatus.RefundedHt, 201)]
    [InlineData(SettleStatus.Calculated, 201)]
    [InlineData(SettleStatus.Refunded, 200)]
    [InlineData(SettleStatus.RefundedFt, 200)]
    public void ExtraMatchRefundedSpecification_IsSatisfiedBy_ReturnsFalse_ForInvalidConditions(SettleStatus status, short sourceChildMatchType)
    {
        // Arrange
        var spec = new ExtraMatchRefundedSpecification();
        var entity = new SourcesConditions
        {
            Resource = new Resource
            {
                SettlementStatusId = (short)status,
                SourceChildMatchType = sourceChildMatchType
            }
        };

        // Act
        var result = spec.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    #endregion
}
