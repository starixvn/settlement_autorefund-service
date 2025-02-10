using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public static class SpecificationFactory
{

    public static ISpecification<SourcesConditions> CreateSpecification(SourcesConditions condition)
    {
        return (condition.SourceAction) switch
        {
            "Refund" => new RefundedSpecification(),
            "CfnRefund" => new ConfirmedRefundSpecification(),
            "CfnRefundSDT" => new ConfirmedRefundSdtSpecification(),
            "Cal" => new CalculatedSpecification(),
            _ => new DefaultSpecification()
        };
    }
}