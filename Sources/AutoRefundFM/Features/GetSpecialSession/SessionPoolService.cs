namespace AutoRefundFM.Features.GetSpecialSession;

using Models;
using System.Collections.Concurrent;

public interface ISessionPoolService
{
    Task<List<SpecialSession>> PeekSessions();

    void RemoveSession(SpecialSession session);
}

public class SessionPoolService(IGetSpecialSessionHandler getSpecialSessionHandler) : ISessionPoolService
{
    private readonly ConcurrentDictionary<SpecialSession, SpecialSession> poolOfSessions = new();

    public List<SpecialSession> PoolOfSessions => [.. poolOfSessions.Values];

    public void RemoveSession(SpecialSession session)
    {
        poolOfSessions.TryRemove(session, out _);
    }

    public async Task<List<SpecialSession>> PeekSessions()
    {
        var sessions = await getSpecialSessionHandler.Handle();

        return [.. sessions
            .Where(session => !poolOfSessions.ContainsKey(session)
                && poolOfSessions.TryAdd(session, session))];
    }
}