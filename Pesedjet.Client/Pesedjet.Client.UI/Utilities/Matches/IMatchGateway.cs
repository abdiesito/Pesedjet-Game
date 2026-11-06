using System.Threading.Tasks;
using Pesedjet.Client.UI.Models.Lobbies;
using Pesedjet.Client.UI.Models.Matches;

namespace Pesedjet.Client.UI.Utilities.Matches;

public interface IMatchGateway
{
    Task<MatchSnapshot> StartAsync(LobbySnapshot lobby);

    Task<TurnOutcome> PlayCardAsync(PlayRequest request);
    
    Task<TurnOutcome> AdvanceAsync();

    Task<MatchSnapshot> StartNextRoundAsync();
}
