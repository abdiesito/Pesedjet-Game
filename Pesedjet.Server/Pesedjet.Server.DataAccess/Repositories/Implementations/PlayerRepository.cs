#nullable enable

using System;
using System.Threading.Tasks;
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

    public async Task<Player?> GetPlayerByIdAsync(Guid playerId)
    {
        Player? player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == playerId);

        return player;
    }

    public async Task<Player?> GetPlayerByUsernameAsync(string username)
    {
        Player? player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Username == username);

        return player;
    }

    public async Task<Player?> GetPlayerByEmailAsync(string email)
    {
        Player? player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == email);

        return player;
    }

    public async Task<bool> CreatePlayerAsync(Player player)
    {
        _context.Players.Add(player);
        int affectedRows = await _context.SaveChangesAsync();

        return affectedRows == ExpectedAffectedRows;
    }
}
