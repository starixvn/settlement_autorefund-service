using AutoRefundFM.Models;
using Fanex.Data.Repository;

namespace AutoRefundFM.Features.GetSpecialSession;

public interface IGetSpecialSessionHandler
{
    Task<List<Match>> Handle();
}

public class GetSpecialSessionHandler : IGetSpecialSessionHandler
{
    private readonly IDynamicRepository repository;

    public GetSpecialSessionHandler(IDynamicRepository repository)
    {
        this.repository = repository;
    }

    public async Task<List<Match>> Handle()
    {
        var results = await repository.FetchAsync<ResultDetail>(new GetSpecialSessionCriteria());

        var matches = results
            .Where(x => x.MatchId > 0)
            .Select(x => new SpecialSession
            {
                MatchId = x.MatchId,
                ResourceId = x.ResourceId,
                StartMinute = int.Parse(x.HomeValue),
                StartSecond = int.Parse(x.OtherInfo.Split(",")[0].Split(":")[1]),
                EndMinute = int.Parse(x.AwayValue),
                EndSecond = int.Parse(x.OtherInfo.Split(",")[1].Split(":")[1]),
            })
            .GroupBy(x => x.MatchId)
            .Select(x => new Match
            {
                MatchId = x.Key,
                SpecialSessions = x.ToList()
            }).ToList();


        return matches;

    }
}
