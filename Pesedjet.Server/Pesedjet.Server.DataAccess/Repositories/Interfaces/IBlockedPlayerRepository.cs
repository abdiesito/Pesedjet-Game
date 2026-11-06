using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Repositories.Interfaces;

public interface IBlockedPlayerRepository
{
    List<BlockedPlayer> GetBlocksBetween(Guid playerId, List<Guid> otherPlayerIds);
}
