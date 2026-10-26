using System.Threading.Tasks;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Matches;

public interface IMatchGateway
{
    Task<MatchSnapshot> StartAsync(RoomSnapshot room);

    Task<TurnOutcome> PlayCardAsync(PlayRequest request);
    
    Task<TurnOutcome> AdvanceAsync();

    Task<MatchSnapshot> StartNextRoundAsync();
}
