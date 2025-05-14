using AutoEarlyRefund.Models;

namespace AutoEarlyRefund.Features.PeekNewMatches;

public class MatchPoolService(GetMatchesService fetchItemsHandler)
{
    private readonly Lock lockObject = new();

    private readonly List<Match> poolOfMatches = [];

    public void Remove(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

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