using System.Collections.Concurrent;

namespace Pesedjet.Server.Contracts.Interfaces;

public interface IActivePlayers
{
    public ConcurrentDictionary<string, bool> ActivePlayersDictionary { get; }
    
    public bool addActivePlayer(string playerName);
    public bool removeActivePlayer(string playerName);
    
}