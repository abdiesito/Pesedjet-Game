using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Models;

public partial class BlockedPlayerItem : ObservableObject
{
    private const int InitialsLength = 2;

    [ObservableProperty]
    private string _blockedOnText = string.Empty;

    public ICommand? UnblockCommand { get; set; }

    public required Guid Id { get; init; }

    public required string Username { get; init; }

    public required DateTimeOffset BlockedAt { get; init; }

    public string Initials => Username[..Math.Min(InitialsLength, Username.Length)];
}
