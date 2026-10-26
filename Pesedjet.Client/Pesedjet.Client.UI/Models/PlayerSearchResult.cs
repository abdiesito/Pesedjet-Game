using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class PlayerSearchResult : ObservableObject
{
    private const int InitialsLength = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSendRequest), nameof(IsRequestSent), nameof(AreFriends), nameof(IsBlocked))]
    private FriendshipRelation _relation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayLabel))]
    private bool _isSending;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayLabel))]
    private string _sendLabel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayLabel))]
    private string _sendingLabel = string.Empty;

    public ICommand? SendRequestCommand { get; set; }

    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public required PresenceStatus Presence { get; init; }

    public DateTimeOffset? LastSeenAt { get; init; }

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];

    public bool IsOnline => Presence == PresenceStatus.Online;

    public bool IsInMatch => Presence == PresenceStatus.InMatch;

    public bool IsOffline => Presence == PresenceStatus.Offline;

    public bool CanSendRequest => Relation == FriendshipRelation.None;

    public bool IsRequestSent => Relation == FriendshipRelation.RequestSent;

    public bool AreFriends => Relation == FriendshipRelation.Friends;

    public bool IsBlocked => Relation == FriendshipRelation.Blocked;

    public string DisplayLabel => IsSending ? SendingLabel : SendLabel;
}
