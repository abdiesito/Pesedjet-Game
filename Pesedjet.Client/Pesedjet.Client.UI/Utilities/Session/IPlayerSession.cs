using System.ComponentModel;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Session;

public interface IPlayerSession : INotifyPropertyChanged
{
    PlayerProfile? CurrentPlayer { get; set; }
    
    string? GuestAlias { get; set; }

    bool IsGuest { get; }
    
    bool HasWelcomed { get; set; }
}
