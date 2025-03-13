using AutoEarlyRefund.Contracts;

namespace AutoEarlyRefund.Helpers;

public static class ConsumerHelper
{
    public static bool IsExtraMatchCalculated(MatchCalculated message)
    {
        return message.ChildMatchType == 201
               && message.FinalAwayScore != message.FinalHomeScore
               && (message.ActionName.Equals("CAL", StringComparison.OrdinalIgnoreCase)
                   || message.ActionName.Equals("CAL-FT", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsPenMatchCalculated(MatchCalculated message)
    {
        return message.ChildMatchType == 202
               && (message.ActionName.Equals("CAL", StringComparison.OrdinalIgnoreCase)
                   || message.ActionName.Equals("CAL-FT", StringComparison.OrdinalIgnoreCase));
    }


    public static bool IsExtraMatchRefunded(MatchRefunded message)
    {
        return message.ChildMatchType == 201
               && (message.ActionName.Equals("REFUND", StringComparison.OrdinalIgnoreCase)
                   || message.ActionName.Equals("REFUND-FT", StringComparison.OrdinalIgnoreCase));
    }


    public static bool TryGetValidResources(List<ResourceItem> resourceItems, out List<ResourceItem> validResources)
    {
        List<string> resources = new() { "383", "384", "385" };

        validResources = resourceItems
            .Where(x => resources.Exists(r => x.ResourceId.StartsWith(r))).ToList();

        return validResources.Count != 0;
    }
}
