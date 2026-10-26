using System;
using System.Windows.Input;

namespace Pesedjet.Client.UI.Models;

public sealed class RoomParticipantItem
{
    private const int InitialsLength = 2;

    public ICommand? KickCommand { get; set; }

    public ICommand? ProposeKickCommand { get; set; }

    public required string Username { get; init; }

    public int? AvatarId { get; init; }

    public bool IsHost { get; init; }

    public bool IsCurrentPlayer { get; init; }

    public bool CanKick { get; init; }

    public bool CanProposeKick { get; init; }

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];

    public string? AvatarIconData => AvatarCatalog.Find(AvatarId)?.IconData;

    public bool HasAvatar => AvatarIconData is not null;
}
