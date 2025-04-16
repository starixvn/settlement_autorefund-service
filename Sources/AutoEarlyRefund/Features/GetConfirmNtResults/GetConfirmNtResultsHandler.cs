using AutoEarlyRefund.Contracts;
using AutoEarlyRefund.Models;
using MassTransit;

namespace AutoEarlyRefund.Features.GetConfirmNtResults;

public class GetConfirmNtResultsHandler(IDynamicRepository repository, IPublishEndpoint publishEndpoint)
{
    public async Task Handle()
    {
        var criteria = new GetConfirmNtResultsCriteria();
        var results = await repository.FetchAsync<ResultDetail>(criteria);

        var matches = results.GroupBy(x => new
        {
            x.MatchId,
            x.LeagueId,
            x.ChildMatchType,
            x.SportType
        }).Select(x => new Match
        {
            MatchId = x.Key.MatchId,
            LeagueId = x.Key.LeagueId,
            ChildMatchType = x.Key.ChildMatchType,
            SportType = x.Key.SportType,
            ResultDetails = [.. x]
        }).ToList();


        foreach (var item in matches)
        {
            await publishEndpoint.Publish(new NotTakenResultsConfirmed(item.MatchId, item.SportType, item.ChildMatchType, [.. item.ResourceItems]));
        }
    }
}
