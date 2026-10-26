using CommunityToolkit.Mvvm.ComponentModel;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Session;

public sealed partial class PlayerSession : ObservableObject, IPlayerSession
{
    [ObservableProperty]
    private PlayerProfile? _currentPlayer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGuest))]
    private string? _guestAlias;

    public bool IsGuest => GuestAlias is not null;

    public bool HasWelcomed { get; set; }
    
    partial void OnCurrentPlayerChanged(PlayerProfile? oldValue, PlayerProfile? newValue)
    {
        if (oldValue is null || newValue is null)
        {
            HasWelcomed = false;
        }
    }
}
