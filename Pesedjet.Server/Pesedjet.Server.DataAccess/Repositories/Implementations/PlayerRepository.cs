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

    public async Task<bool> VerifyCodeAsync(string usernameOrEmail, string code, string purpose)
    {
        bool isValid = false;
        await Task.CompletedTask;

        return isValid;
    }

    public async Task<bool> GeneratePasswordResetCodeAsync(Guid playerId)
    {
        bool isGenerated = false;
        await Task.CompletedTask;

        return isGenerated;
    }

    public async Task<bool> UpdatePasswordHashAsync(Guid playerId, string newPasswordHash)
    {
        bool isUpdated = false;

        Player? player = await _context.Players
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player is not null)
        {
            player.PasswordHash = newPasswordHash;
            int affectedRows = await _context.SaveChangesAsync();
            isUpdated = affectedRows == ExpectedAffectedRows;
        }

        return isUpdated;
    }
}