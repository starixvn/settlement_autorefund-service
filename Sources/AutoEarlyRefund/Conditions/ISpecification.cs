using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
}

public class DefaultSpecification : ISpecification<ResultDetail>
{
    public bool IsSatisfiedBy(ResultDetail entity)
    {
        return true;
    }
}


public class RefundedSpecification : ISpecification<ResultDetail>
{
    public bool IsSatisfiedBy(ResultDetail entity)
    {
        return entity.SettlementStatusId == (short)SettleStatus.Refunded;
    }
}

public class CalculatedSpecification : ISpecification<ResultDetail>
{
    public bool IsSatisfiedBy(ResultDetail entity)
    {
        return entity.SettlementStatusId == (short)SettleStatus.Calculated;
    }
}

public class ConfirmedRefundSdtSpecification : ISpecification<ResultDetail>
{
    public bool IsSatisfiedBy(ResultDetail entity)
    {
        return entity.IsConfirmedRefundSdt;
    }
}

public class ConfirmedRefundSpecification : ISpecification<ResultDetail>
{
    public bool IsSatisfiedBy(ResultDetail entity)
    {
        return entity.IsConfirmedRefund;
    }
}

public class UseCaseSpecification : ISpecification<ResultDetail>
{
    private readonly List<ISpecification<ResultDetail>> conditions;

    public UseCaseSpecification(List<ISpecification<ResultDetail>> conditions)
    {
        this.conditions = conditions;
    }
    public bool IsSatisfiedBy(ResultDetail entity)
    {
        return conditions.All(x => x.IsSatisfiedBy(entity));
    }
}
