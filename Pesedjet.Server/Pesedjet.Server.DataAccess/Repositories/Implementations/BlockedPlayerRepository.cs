using Microsoft.EntityFrameworkCore;
using Pesedjet.Server.DataAccess.Contexts;
using Pesedjet.Server.DataAccess.Entities.Identity;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.DataAccess.Repositories.Implementations;

public class BlockedPlayerRepository : IBlockedPlayerRepository
{
    private readonly PesedjetDatabaseContext _context;

    public BlockedPlayerRepository(PesedjetDatabaseContext context)
    {
        _context = context;
    }

    public List<BlockedPlayer> GetBlocksBetween(Guid playerId, List<Guid> otherPlayerIds)
    {
        List<BlockedPlayer> blocks = _context.BlockedPlayers
            .AsNoTracking()
            .Where(block => (block.PlayerId == playerId && otherPlayerIds.Contains(block.BlockedId))
                            || (block.BlockedId == playerId && otherPlayerIds.Contains(block.PlayerId)))
            .ToList();

        return blocks;
    }
}
