namespace Pesedjet.Client.UI.Models.Account;

public sealed record PlayerProfile(
    string Username,
    int Points,
    int Victories,
    string Description = "",
    int? AvatarId = null);
