using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public static class SpecificationFactory
{

    public static ISpecification<Resource> CreateSpecification(SourcesConditions condition)
    {
        return condition.Conditions switch
        {
            "Refund" => new RefundedSpecification(),
            "CfnRefund" => new ConfirmedRefundSpecification(),
            "CfnRefundSDT" => new ConfirmedRefundSdtSpecification(),
            "Cal" => new CalculatedSpecification(),
            _ => new DefaultSpecification()
        };
    }
}