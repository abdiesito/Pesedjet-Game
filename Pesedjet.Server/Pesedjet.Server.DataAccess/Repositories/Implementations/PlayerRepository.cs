using Microsoft.EntityFrameworkCore;
using Pesedjet.Server.DataAccess.Contexts;
using Pesedjet.Server.DataAccess.Entities.Identity;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.DataAccess.Repositories.Implementations;

public class PlayerRepository : IPlayerRepository
{
    private const int ExpectedAffectedRows = 1;

    private readonly PesedjetDatabaseContext _context;

    public PlayerRepository(PesedjetDatabaseContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetPlayerByUsernameAsync(string username)
    {
        return await _context.Players
            .FirstOrDefaultAsync(player => player.Username == username);
    }

    public async Task<Player?> GetPlayerByEmailAsync(string email)
    {
        return await _context.Players
            .FirstOrDefaultAsync(player => player.Email == email);
    }

    public async Task<bool> CreatePlayerAsync(Player player)
    {
        _context.Players.Add(player);
        int affectedRows = await _context.SaveChangesAsync();

        return affectedRows == ExpectedAffectedRows;
    }
}
