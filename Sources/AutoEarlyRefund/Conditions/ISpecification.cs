using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T entity);
}


public class DefaultSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return false;
    }
}


public class RefundedSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource?.SettlementStatusId == (short)SettleStatus.Refunded;
    }
}

public class CalculatedSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource?.SettlementStatusId == (short)SettleStatus.Calculated;
    }
}

public class ConfirmedRefundSdtSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource is not null && entity.Resource.IsConfirmedRefundSdt;
    }
}

public class ConfirmedRefundSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource is not null && entity.Resource.IsConfirmedRefund;
    }
}

public class ConfirmedRefundNTSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource is not null
            && entity.Resource.ResultStatusId == (int)ResultStatus.Confirm
            && entity.Resource.HomeValue == "NT";
    }
}

public class ParentMatchCalculatedWinLostSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource?.SettlementStatusId == (short)SettleStatus.Calculated
            && !entity.Resource.HomeValue.Equals(entity.Resource.AwayValue);
    }
}

public class ExtraMatchCalculatedWinLostSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource?.SettlementStatusId == (short)SettleStatus.Calculated
            && !entity.Resource.HomeValue.Equals(entity.Resource.AwayValue);
    }
}

public class ExtraMatchRefundedSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return (entity.Resource?.SettlementStatusId == (short)SettleStatus.Refunded || entity.Resource?.SettlementStatusId == (short)SettleStatus.RefundedFt)
            && entity.Resource?.SourceChildMatchType == 201;
    }
}

