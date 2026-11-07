#nullable enable

using System;
using System.ServiceModel;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Callbacks;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false)]
public sealed class LobbyCallbackHandler : ILobbyCallback
{
    public event EventHandler<LobbyMember>? PlayerJoined;

    public event EventHandler<Guid>? PlayerLeft;

    void ILobbyCallback.PlayerJoined(LobbyMember player)
    {
        PlayerJoined?.Invoke(this, player);
    }

    void ILobbyCallback.PlayerLeft(Guid playerId)
    {
        PlayerLeft?.Invoke(this, playerId);
    }
}