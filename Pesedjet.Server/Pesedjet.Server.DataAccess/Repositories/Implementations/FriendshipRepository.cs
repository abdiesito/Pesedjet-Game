using Microsoft.EntityFrameworkCore;
using Pesedjet.Server.DataAccess.Contexts;
using Pesedjet.Server.DataAccess.Entities.Enums;
using Pesedjet.Server.DataAccess.Entities.Identity;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.DataAccess.Repositories.Implementations;

public class FriendshipRepository : IFriendshipRepository
{
    private const int ExpectedAffectedRows = 1;

    private readonly PesedjetDatabaseContext _context;

    public FriendshipRepository(PesedjetDatabaseContext context)
    {
        _context = context;
    }

    public Friendship? GetFriendshipBetween(Guid playerId, Guid otherPlayerId)
    {
        Friendship? friendship = _context.Friendships
            .AsNoTracking()
            .FirstOrDefault(f => (f.PlayerRequesterId == playerId && f.PlayerAddresseeId == otherPlayerId)
                                 || (f.PlayerRequesterId == otherPlayerId && f.PlayerAddresseeId == playerId));

        return friendship;
    }

    public List<Friendship> GetFriendshipsBetween(Guid playerId, List<Guid> otherPlayerIds)
    {
        List<Friendship> friendships = _context.Friendships
            .AsNoTracking()
            .Where(f => (f.PlayerRequesterId == playerId && otherPlayerIds.Contains(f.PlayerAddresseeId))
                        || (f.PlayerAddresseeId == playerId && otherPlayerIds.Contains(f.PlayerRequesterId)))
            .ToList();

        return friendships;
    }

    public List<Friendship> GetAcceptedFriendships(Guid playerId)
    {
        List<Friendship> friendships = _context.Friendships
            .AsNoTracking()
            .Include(f => f.PlayerRequester)
            .Include(f => f.PlayerAddressee)
            .Where(f => (f.PlayerRequesterId == playerId || f.PlayerAddresseeId == playerId)
                        && f.Status == FriendshipStatus.Accepted)
            .ToList();

        return friendships;
    }

    public List<Friendship> GetPendingRequestsReceivedBy(Guid addresseeId)
    {
        List<Friendship> friendships = _context.Friendships
            .AsNoTracking()
            .Include(f => f.PlayerRequester)
            .Where(f => f.PlayerAddresseeId == addresseeId && f.Status == FriendshipStatus.Pending)
            .OrderBy(f => f.PlayerRequester.Username)
            .ToList();

        return friendships;
    }

    public bool CreatePendingFriendship(Guid requesterId, Guid addresseeId)
    {
        Friendship friendship = new Friendship
        {
            PlayerRequesterId = requesterId,
            PlayerAddresseeId = addresseeId,
            Status = FriendshipStatus.Pending
        };

        _context.Friendships.Add(friendship);
        int affectedRows = _context.SaveChanges();

        return affectedRows == ExpectedAffectedRows;
    }

    public bool AcceptPendingFriendship(Guid requesterId, Guid addresseeId)
    {
        int affectedRows = _context.Friendships
            .Where(f => f.PlayerRequesterId == requesterId
                        && f.PlayerAddresseeId == addresseeId
                        && f.Status == FriendshipStatus.Pending)
            .ExecuteUpdate(setters => setters.SetProperty(f => f.Status, FriendshipStatus.Accepted));

        return affectedRows == ExpectedAffectedRows;
    }

    public bool DeletePendingFriendship(Guid requesterId, Guid addresseeId)
    {
        int affectedRows = _context.Friendships
            .Where(f => f.PlayerRequesterId == requesterId
                        && f.PlayerAddresseeId == addresseeId
                        && f.Status == FriendshipStatus.Pending)
            .ExecuteDelete();

        return affectedRows == ExpectedAffectedRows;
    }
}
