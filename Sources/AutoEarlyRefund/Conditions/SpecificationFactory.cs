using AutoEarlyRefund.Models;
namespace AutoEarlyRefund.Conditions;

public static class SpecificationFactory
{

    public static ISpecification<SourcesConditions> CreateSpecification(SourcesConditions condition)
    {
        return condition switch
        {
            { SourceAction: "Refund", SourceMatchType: 3, SourceChildMatchType: 201, Conditions: "0" } => new ExtraMatchRefundedSpecification(),
            { SourceAction: "Refund" } => new RefundedSpecification(),
            { SourceAction: "Cal", SourceMatchType: 2, SourceChildMatchType: 0, Conditions: "1" } => new ParentMatchCalculatedWinLostSpecification(),
            { SourceAction: "Cal", SourceMatchType: 3, SourceChildMatchType: 201, Conditions: "1" } => new ExtraMatchCalculatedWinLostSpecification(),
            { SourceAction: "Cal", SourceMatchType: 1, SourceChildMatchType: 202, Conditions: "0" } => new CalculatedSpecification(),
            { SourceAction: "CfmRefundAdminSite", SourceMatchType: 1, SourceChildMatchType: 202, Conditions: "3" } => new ConfirmedRefundNTSpecification(),
            { SourceAction: "CfmRefundSDT", SourceMatchType: 1, SourceChildMatchType: 202, Conditions: "0" } => new ConfirmedRefundSdtSpecification(),
            { SourceAction: "CfmRefund", SourceMatchType: 1, SourceChildMatchType: 202, Conditions: "0" } => new ConfirmedRefundSpecification(),


            _ => new DefaultSpecification()
        };
    }
}