using AutoRefund.Core.Models;
using AutoRefundFM.Models;
using Fanex.Settlement.Converter.Services;

namespace AutoRefundFM.Services;

public interface IFastMarketDataTransformationService
{
    (BetTypesRoot, string) CreateBetTypesRoot(List<FastMarketResultDetail> fastMarketResultDetails);
    (List<ResourceItem>, string) CreateResourceItemsFromResult(List<FastMarketResultDetail> fastMarketResultDetails, string errorMsg = "");
    string CreateEventCountersXml(List<EventCounter> eventCounters);
}

public class FastMarketDataTransformationService(XmlConvertService xmlConvertService) : IFastMarketDataTransformationService
{
    public string CreateEventCountersXml(List<EventCounter> eventCounters)
    {
        var eventCounter = GetEventCounters(eventCounters);
        var eventCountersRoot = new EventCountersRoot
        {
            EventCounter = eventCounter
        };

        return xmlConvertService.Serialize(eventCountersRoot);
    }

    private List<EventCounterElement> GetEventCounters(List<EventCounter> eventCounters)
    {
        var eventCountersElement = new List<EventCounterElement>();
        if (eventCounters.Count != 0)
        {
            const int positionGoal = 0;
            const int positionCorner = 1;
            const int positionFreeKick = 2;
            const int positionGoalKick = 3;
            const int positionThrowIn = 4;
            const int positionPenalty = 5;

            var items = new EventCounterElement[6];
            items[positionGoal] = EventCounterElement.GoalResult(eventCounters);
            items[positionCorner] = EventCounterElement.CornerResult(eventCounters);
            items[positionFreeKick] = EventCounterElement.FreeKickResult(eventCounters);
            items[positionGoalKick] = EventCounterElement.GoalKickResult(eventCounters);
            items[positionThrowIn] = EventCounterElement.ThrowInResult(eventCounters);
            items[positionPenalty] = EventCounterElement.PenaltyResult(eventCounters);

            eventCountersElement.AddRange(items);
        }

        return eventCountersElement;
    }



    public (BetTypesRoot, string) CreateBetTypesRoot(List<FastMarketResultDetail> fastMarketResultDetails)
    {
        var betTypes = fastMarketResultDetails.GroupBy(g => g.BetType)
            .OrderBy(g => g.Key)
            .Select(g => new BetTypeElement
            {
                Id = g.Key,
                Items = [.. g.Select(x => new ItemElement
                {
                    Position = x.Position,
                    Result = GetResult(x),

                })]
            }).ToList();

        var betTypesRoot = new BetTypesRoot
        {
            BetType = betTypes ?? []
        };

        var betTypesRootXml = xmlConvertService.Serialize(betTypesRoot);

        return (betTypesRoot, betTypesRootXml);
    }

    public (List<ResourceItem>, string) CreateResourceItemsFromResult(List<FastMarketResultDetail> fastMarketResultDetails, string errorMsg = "")
    {
        var resourceItems = fastMarketResultDetails
            .Select(x => new ResourceItem
            {
                HomeValue = GetResult(x),
                OtherInfo = x.Position.ToString(),
                //SourceId = x.SourceId,
                ResourceId = x.BetType.ToString(),
            })
            .ToList();

        var resourceXml = CreateResourceXml(resourceItems);

        return (resourceItems, resourceXml);
    }

    private static string GetResult(FastMarketResultDetail resultDetail)
    {
        return resultDetail.BetType switch
        {
            FastMarketBetType.TimeMachineMinutes or FastMarketBetType.TimeMachineHdp
                => resultDetail.Result?.ToString() ?? string.Empty,

            FastMarketBetType.NextOneMinuteSetPiece
                => resultDetail.ResultItem.Goal
                    ? SetPiece.CornerFreeKickGoalKickAndPenalty.ToString()
                    : resultDetail.ResultItem.Value.ToString(),

            _ => resultDetail.ResultItem.Value.ToString()
        };
    }

    private string CreateResourceXml(List<ResourceItem> resourceItems)
    {
        var resourceRoot = new Resources<ResourceItem>
        {
            ResourceItems = resourceItems
        };

        return xmlConvertService.Serialize(resourceRoot);
    }
}
