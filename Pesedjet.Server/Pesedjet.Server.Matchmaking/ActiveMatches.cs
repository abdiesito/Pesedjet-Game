using System.Collections.Concurrent;
using Pesedjet.Server.Contracts.Interfaces;

namespace Pesedjet.Server.Matchmaking;

public class ActiveMatches : IActiveMatches
{
    public ConcurrentDictionary<string, bool> ActiveMatchesDictionary { get; }

    public ActiveMatches()
    {
        ActiveMatchesDictionary = new ConcurrentDictionary<string, bool>();
    }

    public bool addActiveMatch(string matchToken)
    {
        return ActiveMatchesDictionary.TryAdd(matchToken, true);
    }

    public bool removeActiveMatch(string matchToken)
    {
        return ActiveMatchesDictionary.TryRemove(matchToken, out _);
    }
}