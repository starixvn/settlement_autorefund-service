using AutoEarlyRefund.Helpers;
using AutoRefund.Core.Models;

namespace AutoEarlyRefund.Tests.Helpers
{
    public class ConsumerHelperTests
    {

        [Fact]
        public void TryGetValidResources_ShouldReturnTrueAndValidResources_WhenResourcesAreValid()
        {
            var resourceItems = new List<ResourceItem>
                {
                    new() { ResourceId = "3831" },
                    new() { ResourceId = "3842" },
                    new() { ResourceId = "3853" }
                };

            var result = ConsumerHelper.TryGetValidResources(resourceItems, out var validResources);

            Assert.True(result);
            Assert.Equal(3, validResources.Count);
        }

        [Fact]
        public void TryGetValidResources_ShouldReturnFalse_WhenNoResourcesAreValid()
        {
            var resourceItems = new List<ResourceItem>
                {
                    new() { ResourceId = "1234" },
                    new() { ResourceId = "5678" }
                };

            var result = ConsumerHelper.TryGetValidResources(resourceItems, out var validResources);

            Assert.False(result);
            Assert.Empty(validResources);
        }
    }
}
