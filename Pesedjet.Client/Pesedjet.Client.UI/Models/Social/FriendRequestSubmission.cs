using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record FriendRequestSubmission(PlayerSearchResult Player, FriendRequest Request);
