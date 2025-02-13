using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

public class DefaultSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return true;
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
            && entity.Resource.IsConfirmedRefund
            && entity.Resource.CfmHomeValue == "NT";
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

public class RelatedMatchCalculatedWinLostSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource?.SettlementStatusId == (short)SettleStatus.Calculated
            && !entity.Resource.HomeValue.Equals(entity.Resource.AwayValue);
    }
}

public class UseCaseSpecification : ISpecification<SourcesConditions>
{
    private readonly List<ISpecification<SourcesConditions>> conditions;

    public UseCaseSpecification(List<ISpecification<SourcesConditions>> conditions)
    {
        this.conditions = conditions;
    }
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return conditions.All(x => x.IsSatisfiedBy(entity));
    }
}
