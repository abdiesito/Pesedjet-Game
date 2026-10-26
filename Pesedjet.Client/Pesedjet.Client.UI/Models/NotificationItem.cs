using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class NotificationItem : ObservableObject
{
    private const int InitialsLength = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayLabel))]
    private bool _isProcessing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActions))]
    private bool _isExpired;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private string _timeAgoText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayLabel))]
    private string _primaryLabel = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayLabel))]
    private string _processingLabel = string.Empty;

    public ICommand? AcceptCommand { get; set; }

    public ICommand? RejectCommand { get; set; }

    public required Guid Id { get; init; }

    public required NotificationKind Kind { get; init; }

    public required string SenderName { get; init; }

    public required DateTimeOffset ReceivedAt { get; init; }

    public bool IsNew { get; init; }

    public string RoomCode { get; init; } = string.Empty;

    public int Occupancy { get; init; }

    public int Capacity { get; init; }

    public string SenderInitials => SenderName[..Math.Min(InitialsLength, SenderName.Length)];

    public bool IsRoomInvitation => Kind == NotificationKind.RoomInvitation;

    public bool IsFriendRequest => Kind == NotificationKind.FriendRequest;

    public bool HasActions => !IsExpired;

    public string DisplayLabel => IsProcessing ? ProcessingLabel : PrimaryLabel;
}
