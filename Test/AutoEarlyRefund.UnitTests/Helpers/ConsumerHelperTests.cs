using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Helpers;
using AutoRefund.Core.Models;

namespace AutoEarlyRefund.Tests.Helpers
{
    public class ConsumerHelperTests
    {
        [Fact]
        public void IsExtraMatchCalculated_ShouldReturnTrue_WhenConditionsAreMet()
        {
            var message = new MatchCalculated(1, 1, 1, 201, 2, 1, 2, 1, "CAL");

            var result = ConsumerHelper.IsExtraMatchCalculated(message);

            Assert.True(result);
        }

        [Fact]
        public void IsExtraMatchCalculated_ShouldReturnFalse_WhenConditionsAreNotMet()
        {
            var message = new MatchCalculated(1, 1, 1, 202, 2, 1, 2, 1, "CAL");


            var result = ConsumerHelper.IsExtraMatchCalculated(message);

            Assert.False(result);
        }

        [Fact]
        public void IsPenMatchCalculated_ShouldReturnTrue_WhenConditionsAreMet()
        {
            var message = new MatchCalculated(1, 1, 1, 202, 2, 1, 2, 1, "CAL");


            var result = ConsumerHelper.IsPenMatchCalculated(message);

            Assert.True(result);
        }

        [Fact]
        public void IsPenMatchCalculated_ShouldReturnFalse_WhenConditionsAreNotMet()
        {
            var message = new MatchCalculated(1, 1, 1, 201, 2, 1, 2, 1, "CAL");


            var result = ConsumerHelper.IsPenMatchCalculated(message);

            Assert.False(result);
        }

        [Fact]
        public void IsExtraMatchRefunded_ShouldReturnTrue_WhenConditionsAreMet()
        {
            var message = new MatchRefunded(1, 1, 1, 201, "REFUND");


            var result = ConsumerHelper.IsExtraMatchRefunded(message);

            Assert.True(result);
        }

        [Fact]
        public void IsExtraMatchRefunded_ShouldReturnFalse_WhenConditionsAreNotMet()
        {
            var message = new MatchRefunded(1, 1, 1, 201, "CAL");


            var result = ConsumerHelper.IsExtraMatchRefunded(message);

            Assert.False(result);
        }

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
