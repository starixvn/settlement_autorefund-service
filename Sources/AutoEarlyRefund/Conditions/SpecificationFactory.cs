using AutoEarlyRefund.Models;
using ConditionTuple = (string SourceAction, short? SourceMatchType, short? SourceChildMatchType, string? Conditions);
namespace AutoEarlyRefund.Conditions;

public static class SpecificationFactory
{
    private static readonly Dictionary<ConditionTuple, Func<SourcesConditions, ISpecification<SourcesConditions>>> _specificationFactories = new()
    {
        { ("Refund", null, null, null), _ => new RefundedSpecification() },
        { ("Cal", 2, 0, "1"), _ => new ParentMatchCalculatedWinLostSpecification() } ,
        { ("Cal", 3, 201, "1"), _ => new ParentMatchCalculatedWinLostSpecification() },
         { ("Cal", 1, 202, "0"), _ => new CalculatedSpecification() },
        { ("CfnRefund", 1, 202, "3"), _ => new ConfirmedRefundNTSpecification() },
    };


    public static ISpecification<SourcesConditions> CreateSpecification(SourcesConditions condition)
    {
        return _specificationFactories
            .TryGetValue((condition.SourceAction, condition.SourceMatchType, condition.SourceChildMatchType, condition.Conditions), out var factory)
             ? factory(condition)
             : new DefaultSpecification();
    }
}