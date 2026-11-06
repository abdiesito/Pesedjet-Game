using System;

namespace Pesedjet.Client.UI.Models;

public sealed record RoomMember(Guid PlayerId, string Username, int? AvatarId, bool IsHost);
