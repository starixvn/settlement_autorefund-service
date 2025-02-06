using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Conditions;

public class SpecificationFactory
{

    public static ISpecification<ResultDetail> CreateSpecification(SourcesConditions condition)
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