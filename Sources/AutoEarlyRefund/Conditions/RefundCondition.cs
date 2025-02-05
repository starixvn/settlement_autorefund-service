namespace AutoEarlyRefund.Conditions;

public abstract class RefundCondition : IRefundConditionHandler
{
    protected abstract string Id { get; }
    protected abstract void Process();

    protected virtual bool CanHandle(string id)
    {
        return id == Id;
    }

    public bool Handle(string id)
    {
        if (CanHandle(id))
        {
            Process();
        }

        return true;
    }
}