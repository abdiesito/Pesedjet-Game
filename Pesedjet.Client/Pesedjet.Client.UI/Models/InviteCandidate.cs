using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class InviteCandidate : ObservableObject
{
    private const int InitialsLength = 2;

    [ObservableProperty]
    private bool _isSelected;

    public required string Username { get; init; }

    public required bool IsAvailable { get; init; }

    public PresenceStatus FriendPresence { get; init; }

    public DateTimeOffset? FriendLastSeenAt { get; init; }

    [ObservableProperty]
    private string _statusText = string.Empty;

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];
}
