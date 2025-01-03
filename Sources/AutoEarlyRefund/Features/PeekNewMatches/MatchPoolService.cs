using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.FetchItems;

public class MatchPoolService
{
    private readonly GetMatchesService fetchItemsHandler;

    private readonly object lockObject = new();

    private readonly List<Match> poolOfMatches = [];
    public List<Match> PoolOfMatches => poolOfMatches;

    public MatchPoolService(GetMatchesService fetchItemsHandler)
    {
        this.fetchItemsHandler = fetchItemsHandler;
    }

    public void Remove(Match match)
    {
        lock (lockObject)
        {
            poolOfMatches.RemoveAll(m => m.MatchId == match.MatchId);
        }
    }

    public async Task<List<Match>> PeekNewMatch()
    {
        var matches = await fetchItemsHandler.GetMatches();

        lock (lockObject)
        {
            var newMatches = matches.Except(poolOfMatches).ToList();
            poolOfMatches.AddRange(newMatches);

            return newMatches;
        }
    }

}
