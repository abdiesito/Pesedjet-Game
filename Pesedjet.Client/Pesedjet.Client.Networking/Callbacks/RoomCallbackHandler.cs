#nullable enable

using System;
using System.ServiceModel;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Callbacks;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false)]
public sealed class RoomCallbackHandler : IRoomCallback
{
    public event EventHandler<RoomMember>? PlayerJoined;

    public event EventHandler<Guid>? PlayerLeft;

    void IRoomCallback.PlayerJoined(RoomMember player)
    {
        PlayerJoined?.Invoke(this, player);
    }

    void IRoomCallback.PlayerLeft(Guid playerId)
    {
        PlayerLeft?.Invoke(this, playerId);
    }
}