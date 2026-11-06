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

    public List<Player> SearchPlayersByUsername(string searchTerm, Guid excludedPlayerId, int maximumResults)
    {
        List<Player> players = _context.Players
            .AsNoTracking()
            .Where(p => !p.IsGuest && p.Id != excludedPlayerId && p.Username.Contains(searchTerm))
            .OrderBy(p => p.Username)
            .Take(maximumResults)
            .ToList();

        return players;
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

    // Stores the token only while the player has none, in a single statement so two devices cannot both win.
    public bool TryClaimRememberToken(Guid playerId, string token)
    {
        int affectedRows = _context.Players
            .Where(p => p.Id == playerId && (p.RememberToken == null || p.RememberToken == string.Empty))
            .ExecuteUpdate(setters => setters.SetProperty(p => p.RememberToken, token));

        return affectedRows == ExpectedAffectedRows;
    }

    public Player? GetPlayerByRememberToken(string token)
    {
        Player? player = _context.Players
            .AsNoTracking()
            .FirstOrDefault(p => p.RememberToken == token);

        return player;
    }

    public bool RevokeRememberToken(string token)
    {
        int affectedRows = _context.Players
            .Where(p => p.RememberToken == token)
            .ExecuteUpdate(setters => setters.SetProperty(p => p.RememberToken, (string?)null));

        return affectedRows == ExpectedAffectedRows;
    }
}