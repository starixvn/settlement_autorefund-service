namespace AutoEarlyRefund.Helpers;

public static class ConsumerHelper
{
    public static bool TryGetValidResources(List<ResourceItem> resourceItems, out List<ResourceItem> validResources)
    {
        List<string> resources = ["383", "384", "385"];

        validResources = resourceItems
            .Where(x => resources.Exists(r => x.ResourceId.StartsWith(r))).ToList();

        return validResources.Count != 0;
    }
}
