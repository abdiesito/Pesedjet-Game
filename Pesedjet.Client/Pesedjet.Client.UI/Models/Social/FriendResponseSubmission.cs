using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.Models.Social;

public sealed record FriendResponseSubmission(NotificationItem Item, RespondFriendRequest Request);
