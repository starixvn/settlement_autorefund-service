using AutoEarlyRefund.Models;
namespace AutoEarlyRefund.Conditions;

public static class SpecificationFactory
{

    public static ISpecification<SourcesConditions> CreateSpecification(SourcesConditions condition)
    {
        return condition switch
        {
            { SourceAction: "Refund" } => new RefundedSpecification(),
            { SourceAction: "Cal", SourceMatchType: 2, SourceChildMatchType: 0, Conditions: "1" } => new ParentMatchCalculatedWinLostSpecification(),
            { SourceAction: "Cal", SourceMatchType: 3, SourceChildMatchType: 201, Conditions: "1" } => new RelatedMatchCalculatedWinLostSpecification(),
            { SourceAction: "Cal", SourceMatchType: 1, SourceChildMatchType: 202, Conditions: "0" } => new CalculatedSpecification(),
            { SourceAction: "CfnRefund", SourceMatchType: 1, SourceChildMatchType: 202, Conditions: "3" } => new ConfirmedRefundNTSpecification(),
            _ => new DefaultSpecification()
        };
    }
}