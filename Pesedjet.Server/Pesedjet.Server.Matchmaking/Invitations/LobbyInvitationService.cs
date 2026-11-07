using Pesedjet.Server.Contracts.Internal;
using CoreWCF;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.DataAccess.Entities.Identity;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.Matchmaking.Invitations;

public class LobbyInvitationService : ILobbyInvitationService
{
    private const int MaximumInvitationsPerRequest = 5;

    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IBlockedPlayerRepository _blockedPlayerRepository;
    private readonly ILobbyDirectory _lobbyDirectory;
    private readonly IFriendshipNotifier _notifier;

    public LobbyInvitationService(LobbyInvitationServiceDependencies dependencies)
    {
        _friendshipRepository = dependencies.FriendshipRepository;
        _blockedPlayerRepository = dependencies.BlockedPlayerRepository;
        _lobbyDirectory = dependencies.LobbyDirectory;
        _notifier = dependencies.Notifier;
    }

    public List<FriendEntry> GetInvitableFriends(InvitableFriendsRequest request)
    {
        bool isValid = request is not null && request.PlayerId != Guid.Empty;

        if (!isValid)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        LobbyOverview lobby = FindLobbyOfMember(request!.LobbyCode, request.PlayerId);

        return FindInvitableFriends(request.PlayerId, lobby);
    }

    public List<Guid> SendLobbyInvitations(LobbyInvitationRequest request)
    {
        List<Guid> invitedIds = new List<Guid>();

        EnsureInvitationRequestIsValid(request);

        LobbyOverview lobby = FindLobbyOfMember(request.LobbyCode, request.SenderId);
        LobbyMember sender = lobby.Members.First(member => member.PlayerId == request.SenderId);
        int freeSlots = lobby.MaxPlayers - lobby.Members.Count;

        if (!lobby.IsWaiting)
        {
            throw CreateFault(ServiceErrorCodes.LobbyAlreadyStarted, ServiceMessageKeys.LobbyAlreadyStarted);
        }

        if (sender.IsGuest)
        {
            throw CreateFault(ServiceErrorCodes.LobbyInviteNotAllowed, ServiceMessageKeys.LobbyInviteNotAllowed);
        }

        if (freeSlots <= 0)
        {
            throw CreateFault(ServiceErrorCodes.LobbyFull, ServiceMessageKeys.LobbyFull);
        }

        List<FriendEntry> invitableFriends = FindInvitableFriends(request.SenderId, lobby)
            .Where(friend => request.TargetIds.Contains(friend.PlayerId))
            .ToList();

        foreach (FriendEntry friend in invitableFriends)
        {
            bool isRegistered = invitedIds.Count < freeSlots
                                && _lobbyDirectory.RegisterInvitation(lobby.LobbyCode, friend.PlayerId, sender.PlayerId);

            if (isRegistered)
            {
                invitedIds.Add(friend.PlayerId);
                _notifier.NotifyLobbyInvitationReceived(friend.PlayerId, CreateEntry(lobby, sender.Username, sender.PlayerId));
            }
        }

        return invitedIds;
    }

    public List<LobbyInvitationEntry> GetPendingLobbyInvitations(Guid playerId)
    {
        List<LobbyInvitationEntry> entries = new List<LobbyInvitationEntry>();

        if (playerId == Guid.Empty)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        foreach (LobbyOverview lobby in _lobbyDirectory.FindLobbiesInviting(playerId))
        {
            Guid senderId = lobby.Invitations[playerId];
            LobbyMember? sender = lobby.Members.FirstOrDefault(member => member.PlayerId == senderId)
                                  ?? lobby.Members.FirstOrDefault(member => member.PlayerId == lobby.HostPlayerId);

            entries.Add(CreateEntry(lobby, sender?.Username ?? string.Empty, senderId));
        }

        return entries;
    }

    public bool DeclineLobbyInvitation(DeclineLobbyInvitationRequest request)
    {
        bool isValid = request is not null && request.PlayerId != Guid.Empty;

        if (!isValid)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        bool isDeclined = _lobbyDirectory.CancelInvitation(request!.LobbyCode, request.PlayerId);

        return isDeclined;
    }

    private static FaultException<ServiceFault> CreateFault(string errorCode, string messageKey)
    {
        ServiceFault fault = new ServiceFault
        {
            ErrorCode = errorCode,
            MessageKey = messageKey
        };

        return new FaultException<ServiceFault>(fault, new FaultReason(errorCode));
    }

    private static void EnsureInvitationRequestIsValid(LobbyInvitationRequest request)
    {
        bool isValid = request is not null
                       && request.SenderId != Guid.Empty
                       && request.TargetIds is not null
                       && request.TargetIds.Count > 0
                       && request.TargetIds.Count <= MaximumInvitationsPerRequest
                       && !request.TargetIds.Contains(request.SenderId);

        if (!isValid)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }
    }

    private static LobbyInvitationEntry CreateEntry(LobbyOverview lobby, string senderUsername, Guid senderId)
    {
        return new LobbyInvitationEntry
        {
            LobbyCode = lobby.LobbyCode,
            SenderId = senderId,
            SenderUsername = senderUsername,
            CurrentPlayers = lobby.Members.Count,
            MaxPlayers = lobby.MaxPlayers
        };
    }

    private static FriendEntry CreateFriendEntry(Guid playerId, Friendship friendship)
    {
        Player friend = friendship.PlayerRequesterId == playerId
            ? friendship.PlayerAddressee
            : friendship.PlayerRequester;

        return new FriendEntry
        {
            PlayerId = friend.Id,
            Username = friend.Username
        };
    }

    private LobbyOverview FindLobbyOfMember(string lobbyCode, Guid playerId)
    {
        LobbyOverview? lobby = _lobbyDirectory.FindLobby(lobbyCode);

        if (lobby is null)
        {
            throw CreateFault(ServiceErrorCodes.LobbyNotFound, ServiceMessageKeys.LobbyNotFound);
        }

        if (lobby.Members.All(member => member.PlayerId != playerId))
        {
            throw CreateFault(ServiceErrorCodes.LobbyInviteNotAllowed, ServiceMessageKeys.LobbyInviteNotAllowed);
        }

        return lobby;
    }

    private List<FriendEntry> FindInvitableFriends(Guid playerId, LobbyOverview lobby)
    {
        List<Friendship> friendships = _friendshipRepository.GetAcceptedFriendships(playerId);
        List<FriendEntry> friends = friendships
            .Select(friendship => CreateFriendEntry(playerId, friendship))
            .ToList();
        List<Guid> friendIds = friends.Select(friend => friend.PlayerId).ToList();
        List<BlockedPlayer> blocks = _blockedPlayerRepository.GetBlocksBetween(playerId, friendIds);

        List<FriendEntry> invitableFriends = friends
            .Where(friend => lobby.Members.All(member => member.PlayerId != friend.PlayerId))
            .Where(friend => !lobby.Invitations.ContainsKey(friend.PlayerId))
            .Where(friend => blocks.All(block => block.PlayerId != friend.PlayerId && block.BlockedId != friend.PlayerId))
            .Where(friend => _notifier.IsOnline(friend.PlayerId))
            .Where(friend => !_lobbyDirectory.IsInAnyLobby(friend.PlayerId))
            .OrderBy(friend => friend.Username)
            .ToList();

        return invitableFriends;
    }
}
