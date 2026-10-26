namespace Pesedjet.Client.UI.Models;

public sealed record PlayerProfile(
    string Username,
    int Points,
    int Victories,
    string Description = "",
    int? AvatarId = null);
