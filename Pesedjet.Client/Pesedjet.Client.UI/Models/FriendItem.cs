using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class FriendItem : ObservableObject
{
    private const int InitialsLength = 2;

    [ObservableProperty]
    private string _statusText = string.Empty;

    public ICommand? RemoveCommand { get; set; }

    public ICommand? BlockCommand { get; set; }

    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public required PresenceStatus Presence { get; init; }

    public DateTimeOffset? LastSeenAt { get; init; }

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];

    public bool IsOnline => Presence == PresenceStatus.Online;

    public bool IsInMatch => Presence == PresenceStatus.InMatch;

    public bool IsOffline => Presence == PresenceStatus.Offline;
}
