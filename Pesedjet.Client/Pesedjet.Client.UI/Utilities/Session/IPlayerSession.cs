using System;
using System.ComponentModel;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Session;

public interface IPlayerSession : INotifyPropertyChanged
{
    Guid PlayerId { get; set; }

    PlayerProfile? CurrentPlayer { get; set; }
    
    string? GuestAlias { get; set; }

    bool IsGuest { get; }
    
    bool HasWelcomed { get; set; }
}
