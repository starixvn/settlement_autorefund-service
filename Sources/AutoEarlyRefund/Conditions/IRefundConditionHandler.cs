namespace AutoEarlyRefund.Conditions;

internal interface IRefundConditionHandler
{
    bool Handle(string id);
}
