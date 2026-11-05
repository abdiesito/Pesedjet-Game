using System.Collections.Concurrent;

namespace Pesedjet.Server.Contracts.Interfaces;

public interface IActiveMatches
{
    public ConcurrentDictionary<string, bool> ActiveMatchesDictionary { get; }
    
    public bool addActiveMatch(string matchToken);
    public bool removeActiveMatch(string matchToken);
}