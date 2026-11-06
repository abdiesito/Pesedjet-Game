using System.ServiceModel;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Callbacks;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Single, UseSynchronizationContext = false)]
public sealed class FriendshipCallbackHandler : IFriendshipCallback
{
    public event EventHandler<FriendRequestEntry>? FriendRequestReceived;

    public event EventHandler<FriendEntry>? FriendshipEstablished;

    public event EventHandler<LobbyInvitationEntry>? LobbyInvitationReceived;

    public event EventHandler<string>? LobbyInvitationWithdrawn;

    void IFriendshipCallback.OnFriendRequestReceived(FriendRequestEntry request)
    {
        FriendRequestReceived?.Invoke(this, request);
    }

    void IFriendshipCallback.OnFriendshipEstablished(FriendEntry friend)
    {
        FriendshipEstablished?.Invoke(this, friend);
    }

    void IFriendshipCallback.OnLobbyInvitationReceived(LobbyInvitationEntry invitation)
    {
        LobbyInvitationReceived?.Invoke(this, invitation);
    }

    void IFriendshipCallback.OnLobbyInvitationWithdrawn(string lobbyCode)
    {
        LobbyInvitationWithdrawn?.Invoke(this, lobbyCode);
    }
}
