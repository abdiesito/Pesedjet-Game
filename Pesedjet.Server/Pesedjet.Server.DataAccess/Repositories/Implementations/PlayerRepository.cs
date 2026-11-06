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

    public Player? GetPlayerById(Guid playerId)
    {
        Player? player = _context.Players
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == playerId);

        return player;
    }

    public Player? GetPlayerByUsername(string username)
    {
        Player? player = _context.Players
            .AsNoTracking()
            .FirstOrDefault(p => p.Username == username);

        return player;
    }

    public Player? GetPlayerByEmail(string email)
    {
        Player? player = _context.Players
            .AsNoTracking()
            .FirstOrDefault(p => p.Email == email);

        return player;
    }

    public bool CreatePlayer(Player player)
    {
        _context.Players.Add(player);
        int affectedRows = _context.SaveChanges();

        return affectedRows == ExpectedAffectedRows;
    }

    public bool VerifyCode(string usernameOrEmail, string code, string purpose)
    {
        bool isValid = false;

        return isValid;
    }

    public bool GeneratePasswordResetCode(Guid playerId)
    {
        bool isGenerated = false;

        return isGenerated;
    }

    public bool UpdatePasswordHash(Guid playerId, string newPasswordHash)
    {
        bool isUpdated = false;

        Player? player = _context.Players
            .FirstOrDefault(p => p.Id == playerId);

        if (player is not null)
        {
            player.PasswordHash = newPasswordHash;
            int affectedRows = _context.SaveChanges();
            isUpdated = affectedRows == ExpectedAffectedRows;
        }

        return isUpdated;
    }

    public bool SaveRememberToken(Guid playerId, string token)
    {
        bool isSaved = false;

        Player? player = _context.Players
            .FirstOrDefault(p => p.Id == playerId);

        if (player is not null)
        {
            player.RememberToken = token;
            int affectedRows = _context.SaveChanges();
            isSaved = affectedRows == ExpectedAffectedRows;
        }

        return isSaved;
    }

    public Player? GetPlayerByRememberToken(string token)
    {
        Player? player = _context.Players
            .AsNoTracking()
            .FirstOrDefault(p => p.RememberToken == token);

        return player;
    }

    public bool RevokeRememberToken(string username)
    {
        bool isRevoked = false;

        Player? player = _context.Players
            .FirstOrDefault(p => p.Username == username);

        if (player is not null)
        {
            player.RememberToken = null;
            int affectedRows = _context.SaveChanges();
            isRevoked = affectedRows == ExpectedAffectedRows;
        }

        return isRevoked;
    }
}