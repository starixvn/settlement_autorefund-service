using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T entity);
}


public class AllSpecification<T> : ISpecification<T>
{
    private readonly List<ISpecification<T>> specifications;

    public AllSpecification(params List<ISpecification<T>> specifications)
    {
        this.specifications = specifications;
    }

    public bool IsSatisfiedBy(T entity)
    {
        return specifications.All(spec => spec.IsSatisfiedBy(entity));
    }
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

public class RelatedMatchCalculatedWinLostSpecification : ISpecification<SourcesConditions>
{
    public bool IsSatisfiedBy(SourcesConditions entity)
    {
        return entity.Resource?.SettlementStatusId == (short)SettleStatus.Calculated
            && !entity.Resource.HomeValue.Equals(entity.Resource.AwayValue);
    }
}
