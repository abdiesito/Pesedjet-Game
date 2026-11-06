namespace Pesedjet.Server.PlayerManagement.Authentication;

using DataAccess.Repositories.Interfaces;

public class TokenInvalidator : ITokenInvalidator
{
    private readonly IPlayerRepository _playerRepository;

    public TokenInvalidator(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public bool InvalidateToken(string rememberToken)
    {
        bool isInvalidated = false;

        if (!string.IsNullOrWhiteSpace(rememberToken))
        {
            isInvalidated = _playerRepository.RevokeRememberToken(rememberToken);
        }

        return isInvalidated;
    }
}