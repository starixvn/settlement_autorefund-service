using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

public class DefaultSpecification : ISpecification<Resource>
{
    public bool IsSatisfiedBy(Resource entity)
    {
        return true;
    }
}


public class RefundedSpecification : ISpecification<Resource>
{
    public bool IsSatisfiedBy(Resource entity)
    {
        return entity.SettlementStatusId == (short)SettleStatus.Refunded;
    }
}

public class CalculatedSpecification : ISpecification<Resource>
{
    public bool IsSatisfiedBy(Resource entity)
    {
        return entity.SettlementStatusId == (short)SettleStatus.Calculated;
    }
}

public class ConfirmedRefundSdtSpecification : ISpecification<Resource>
{
    public bool IsSatisfiedBy(Resource entity)
    {
        return entity.IsConfirmedRefundSdt;
    }
}

public class ConfirmedRefundSpecification : ISpecification<Resource>
{
    public bool IsSatisfiedBy(Resource entity)
    {
        return entity.IsConfirmedRefund;
    }
}

public class UseCaseSpecification : ISpecification<Resource>
{
    private readonly List<ISpecification<Resource>> conditions;

    public UseCaseSpecification(List<ISpecification<Resource>> conditions)
    {
        this.conditions = conditions;
    }
    public bool IsSatisfiedBy(Resource entity)
    {
        return conditions.All(x => x.IsSatisfiedBy(entity));
    }
}
