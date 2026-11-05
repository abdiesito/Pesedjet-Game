using System.Collections.Concurrent;
using Pesedjet.Server.Contracts.Interfaces;

public class ActivePlayers : IActivePlayers
{
    public ConcurrentDictionary<string, bool> ActivePlayersDictionary { get; }

    public ActivePlayers()
    {
        ActivePlayersDictionary = new ConcurrentDictionary<string, bool>();
    }

    public bool addActivePlayer(string playerName)
    {
        return ActivePlayersDictionary.TryAdd(playerName, true);
    }

    public bool removeActivePlayer(string playerName)
    {
        return ActivePlayersDictionary.TryRemove(playerName, out _);
    }
}