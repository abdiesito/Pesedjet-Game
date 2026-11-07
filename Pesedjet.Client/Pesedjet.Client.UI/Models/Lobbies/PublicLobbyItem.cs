using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.Models.Lobbies;

public partial class PublicLobbyItem : ObservableObject
{
    private const int InitialsLength = 2;
    private const string OccupancyKey = "PublicLobbies.Occupancy";
    private const string JoinKey = "GameMenu.Join";
    private const string JoiningKey = "GameMenu.Joining";
    private const string FullKey = "PublicLobbies.Full";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(JoinLabel))]
    private bool _isJoining;

    public ICommand? JoinCommand { get; set; }

    public required string Code { get; init; }

    public required string HostUsername { get; init; }

    public required int CurrentPlayers { get; init; }

    public required int MaxPlayers { get; init; }

    public bool IsFull => CurrentPlayers >= MaxPlayers;

    public string Initials => HostUsername[..Math.Min(InitialsLength, HostUsername.Length)];

    public string OccupancyText => string.Format(
        System.Globalization.CultureInfo.CurrentCulture,
        LocalizationManager.Instance[OccupancyKey],
        CurrentPlayers,
        MaxPlayers);

    public string JoinLabel => IsJoining
        ? LocalizationManager.Instance[JoiningKey]
        : LocalizationManager.Instance[IsFull ? FullKey : JoinKey];

    // The texts follow the language: the list view model calls this when the language changes.
    public void Refresh()
    {
        OnPropertyChanged(nameof(OccupancyText));
        OnPropertyChanged(nameof(JoinLabel));
    }
}
