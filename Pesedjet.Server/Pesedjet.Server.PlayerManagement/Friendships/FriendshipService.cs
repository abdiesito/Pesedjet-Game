using Pesedjet.Server.Contracts.Internal;
using CoreWCF;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.DataAccess.Entities.Enums;
using Pesedjet.Server.DataAccess.Entities.Identity;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.PlayerManagement.Friendships;

public class FriendshipService : IFriendshipService
{
    private const int MinimumSearchLength = 3;
    private const int MaximumSearchResults = 20;

    private readonly IPlayerRepository _playerRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IBlockedPlayerRepository _blockedPlayerRepository;
    private readonly IFriendshipNotifier _notifier;

    public FriendshipService(FriendshipServiceDependencies dependencies)
    {
        _playerRepository = dependencies.PlayerRepository;
        _friendshipRepository = dependencies.FriendshipRepository;
        _blockedPlayerRepository = dependencies.BlockedPlayerRepository;
        _notifier = dependencies.Notifier;
    }

    public List<FriendEntry> GetFriends(Guid playerId)
    {
        List<FriendEntry> entries = new List<FriendEntry>();

        if (playerId == Guid.Empty)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        List<Friendship> friendships = _friendshipRepository.GetAcceptedFriendships(playerId);

        foreach (Friendship friendship in friendships)
        {
            entries.Add(CreateFriendEntry(playerId, friendship));
        }

        return entries.OrderBy(entry => entry.Username).ToList();
    }

    public List<PlayerSearchEntry> SearchPlayers(PlayerSearchRequest request)
    {
        List<PlayerSearchEntry> entries = new List<PlayerSearchEntry>();

        EnsureSearchRequestIsValid(request);

        string searchTerm = request.SearchTerm.Trim();
        List<Player> players = _playerRepository.SearchPlayersByUsername(
            searchTerm,
            request.RequesterId,
            MaximumSearchResults);
        List<Guid> playerIds = players.Select(player => player.Id).ToList();
        List<BlockedPlayer> blocks = _blockedPlayerRepository.GetBlocksBetween(request.RequesterId, playerIds);
        List<Friendship> friendships = _friendshipRepository.GetFriendshipsBetween(request.RequesterId, playerIds);

        foreach (Player player in players)
        {
            bool hasBlockedRequester = blocks.Any(block => block.PlayerId == player.Id);

            if (!hasBlockedRequester)
            {
                PlayerRelation relation = ResolveRelation(request.RequesterId, player.Id, blocks, friendships);
                entries.Add(CreateSearchEntry(player, relation));
            }
        }

        return entries;
    }

    public PlayerRelation SendFriendRequest(FriendRequest request)
    {
        PlayerRelation relation = PlayerRelation.None;

        EnsureFriendRequestIsValid(request);

        Player sender = FindRegisteredPlayer(request.SenderId);
        Player target = FindRegisteredPlayer(request.TargetId);
        EnsurePlayersAreNotBlocked(request);

        Friendship? existingFriendship = _friendshipRepository.GetFriendshipBetween(request.SenderId, request.TargetId);

        if (existingFriendship is null)
        {
            relation = CreatePendingFriendship(sender, target);
        }
        else if (existingFriendship.Status == FriendshipStatus.Accepted)
        {
            throw CreateFault(ServiceErrorCodes.AlreadyFriends, ServiceMessageKeys.AlreadyFriends);
        }
        else if (existingFriendship.PlayerRequesterId == request.SenderId)
        {
            throw CreateFault(ServiceErrorCodes.FriendRequestAlreadySent, ServiceMessageKeys.FriendRequestAlreadySent);
        }
        else
        {
            relation = AcceptCrossedRequest(existingFriendship, sender, target);
        }

        return relation;
    }

    public List<FriendRequestEntry> GetPendingFriendRequests(Guid playerId)
    {
        List<FriendRequestEntry> entries = new List<FriendRequestEntry>();

        if (playerId == Guid.Empty)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        List<Friendship> pendingFriendships = _friendshipRepository.GetPendingRequestsReceivedBy(playerId);

        foreach (Friendship friendship in pendingFriendships)
        {
            entries.Add(CreateRequestEntry(friendship));
        }

        return entries;
    }

    public bool RespondFriendRequest(RespondFriendRequest request)
    {
        bool isResponded = false;

        EnsureResponseIsValid(request);

        Friendship? friendship = _friendshipRepository.GetFriendshipBetween(request.SenderId, request.ResponderId);
        bool isPendingForResponder = friendship is not null
                                     && friendship.Status == FriendshipStatus.Pending
                                     && friendship.PlayerRequesterId == request.SenderId;

        if (!isPendingForResponder)
        {
            throw CreateFault(ServiceErrorCodes.FriendRequestNotFound, ServiceMessageKeys.FriendRequestNotFound);
        }

        if (request.Accept)
        {
            isResponded = AcceptFriendRequest(request);
        }
        else
        {
            isResponded = _friendshipRepository.DeletePendingFriendship(request.SenderId, request.ResponderId);
        }

        return isResponded;
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

    private static PlayerRelation ResolveRelation(
        Guid requesterId,
        Guid otherPlayerId,
        List<BlockedPlayer> blocks,
        List<Friendship> friendships)
    {
        PlayerRelation relation = PlayerRelation.None;

        bool isBlockedByRequester = blocks.Any(
            block => block.PlayerId == requesterId && block.BlockedId == otherPlayerId);
        Friendship? friendship = friendships.FirstOrDefault(
            candidate => candidate.PlayerRequesterId == otherPlayerId || candidate.PlayerAddresseeId == otherPlayerId);

        if (isBlockedByRequester)
        {
            relation = PlayerRelation.Blocked;
        }
        else if (friendship is not null && friendship.Status == FriendshipStatus.Accepted)
        {
            relation = PlayerRelation.Friends;
        }
        else if (friendship is not null && friendship.PlayerRequesterId == requesterId)
        {
            relation = PlayerRelation.RequestSent;
        }

        return relation;
    }

    private static PlayerSearchEntry CreateSearchEntry(Player player, PlayerRelation relation)
    {
        return new PlayerSearchEntry
        {
            PlayerId = player.Id,
            Username = player.Username,
            Relation = relation
        };
    }

    private static FriendRequestEntry CreateRequestEntry(Friendship friendship)
    {
        return new FriendRequestEntry
        {
            SenderId = friendship.PlayerRequesterId,
            SenderUsername = friendship.PlayerRequester.Username
        };
    }

    private static void EnsureSearchRequestIsValid(PlayerSearchRequest request)
    {
        bool isValid = request is not null
                       && request.RequesterId != Guid.Empty
                       && request.SearchTerm is not null
                       && request.SearchTerm.Trim().Length >= MinimumSearchLength;

        if (!isValid)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }
    }

    private static void EnsureFriendRequestIsValid(FriendRequest request)
    {
        bool isValid = request is not null
                       && request.SenderId != Guid.Empty
                       && request.TargetId != Guid.Empty
                       && request.SenderId != request.TargetId;

        if (!isValid)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }
    }

    private static void EnsureResponseIsValid(RespondFriendRequest request)
    {
        bool isValid = request is not null
                       && request.ResponderId != Guid.Empty
                       && request.SenderId != Guid.Empty
                       && request.ResponderId != request.SenderId;

        if (!isValid)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }
    }

    private static FriendEntry CreateFriendEntry(Guid playerId, Friendship friendship)
    {
        Player friend = friendship.PlayerRequesterId == playerId
            ? friendship.PlayerAddressee
            : friendship.PlayerRequester;

        return CreateFriendEntry(friend);
    }

    private static FriendEntry CreateFriendEntry(Player player)
    {
        return new FriendEntry
        {
            PlayerId = player.Id,
            Username = player.Username
        };
    }

    private Player FindRegisteredPlayer(Guid playerId)
    {
        Player? player = _playerRepository.GetPlayerById(playerId);

        if (player is null || player.IsGuest)
        {
            throw CreateFault(ServiceErrorCodes.UserNotFound, ServiceMessageKeys.UserNotFound);
        }

        return player;
    }

    private void EnsurePlayersAreNotBlocked(FriendRequest request)
    {
        List<Guid> targetIds = new List<Guid> { request.TargetId };
        List<BlockedPlayer> blocks = _blockedPlayerRepository.GetBlocksBetween(request.SenderId, targetIds);

        if (blocks.Count > 0)
        {
            throw CreateFault(ServiceErrorCodes.FriendshipBlocked, ServiceMessageKeys.FriendshipBlocked);
        }
    }

    private PlayerRelation CreatePendingFriendship(Player sender, Player target)
    {
        bool isCreated = _friendshipRepository.CreatePendingFriendship(sender.Id, target.Id);

        if (!isCreated)
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        FriendRequestEntry entry = new FriendRequestEntry
        {
            SenderId = sender.Id,
            SenderUsername = sender.Username
        };
        _notifier.NotifyFriendRequestReceived(target.Id, entry);

        return PlayerRelation.RequestSent;
    }

    private PlayerRelation AcceptCrossedRequest(Friendship crossedFriendship, Player sender, Player target)
    {
        bool isAccepted = _friendshipRepository.AcceptPendingFriendship(
            crossedFriendship.PlayerRequesterId,
            crossedFriendship.PlayerAddresseeId);

        if (!isAccepted)
        {
            throw CreateFault(ServiceErrorCodes.FriendRequestNotFound, ServiceMessageKeys.FriendRequestNotFound);
        }

        _notifier.NotifyFriendshipEstablished(sender.Id, CreateFriendEntry(target));
        _notifier.NotifyFriendshipEstablished(target.Id, CreateFriendEntry(sender));

        return PlayerRelation.Friends;
    }

    private bool AcceptFriendRequest(RespondFriendRequest request)
    {
        bool isAccepted = _friendshipRepository.AcceptPendingFriendship(request.SenderId, request.ResponderId);

        if (isAccepted)
        {
            Player responder = FindRegisteredPlayer(request.ResponderId);
            _notifier.NotifyFriendshipEstablished(request.SenderId, CreateFriendEntry(responder));
        }

        return isAccepted;
    }
}
