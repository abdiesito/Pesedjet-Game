namespace Pesedjet.Server.PlayerManagement.Authentication;

using System.Threading.Tasks;
using DataAccess.Repositories.Interfaces;

public class TokenInvalidator : ITokenInvalidator
{
    private readonly IPlayerRepository _playerRepository;

    public TokenInvalidator(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<bool> InvalidateTokenAsync(string rememberToken)
    {
        bool isInvalidated = false;

        if (!string.IsNullOrWhiteSpace(rememberToken))
        {
            isInvalidated = await _playerRepository.RevokeRememberTokenAsync(rememberToken);
        }

        return isInvalidated;
    }
}