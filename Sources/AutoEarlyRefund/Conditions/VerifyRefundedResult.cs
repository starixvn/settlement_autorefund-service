namespace AutoEarlyRefund.Conditions;

public class VerifyRefundedResult : RefundCondition
{
    protected override string Id { get; } = "VerifyRefundedResult";

    protected override void Process()
    {
        throw new NotImplementedException();
    }
}

