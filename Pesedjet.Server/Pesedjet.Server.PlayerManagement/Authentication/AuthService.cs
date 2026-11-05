using System.ServiceModel;
using System.Threading.Tasks;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.CrossCutting.Security;
using Pesedjet.Server.DataAccess.Entities.Identity;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.PlayerManagement.Authentication;

public class AuthService : IAuthService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITokenInvalidator _tokenInvalidator;
    private readonly IActivePlayers _activePlayers;

    public AuthService(
        IPlayerRepository playerRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        ITokenInvalidator tokenInvalidator,
        IActivePlayers activePlayers)
    {
        _playerRepository = playerRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _tokenInvalidator = tokenInvalidator;
        _activePlayers = activePlayers;
    }

    public async Task<UserSession> LoginAsync(UserLoginRequest request)
    {
        UserSession session = null!;

        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = await _playerRepository.GetPlayerByUsernameAsync(request.Username);
        if (player is null)
        {
            throw CreateFault(ServiceErrorCodes.InvalidCredentials, ServiceMessageKeys.InvalidCredentials);
        }

        bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, player.PasswordHash, player.Id);
        if (!isPasswordValid)
        {
            throw CreateFault(ServiceErrorCodes.InvalidCredentials, ServiceMessageKeys.InvalidCredentials);
        }

        string rememberToken = String.Empty;
        if (request.RememberSession)
        {
            rememberToken = _tokenGenerator.GenerateSessionToken();
            await _playerRepository.SaveRememberTokenAsync(player.Id, rememberToken);
        }

        session = new UserSession
        {
            Username = player.Username,
            RememberToken = rememberToken,
            PreferredLanguage = "es-MX",
            IsGuest = false
        };

        return session;
    }

    public async Task<UserSession> ResumeSessionAsync(string rememberToken)
    {
        UserSession session = null!;

        if (string.IsNullOrWhiteSpace(rememberToken))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = await _playerRepository.GetPlayerByRememberTokenAsync(rememberToken);
        if (player is null)
        {
            throw CreateFault(ServiceErrorCodes.UserNotFound, ServiceMessageKeys.UserNotFound);
        }

        session = new UserSession
        {
            Username = player.Username,
            RememberToken = rememberToken,
            PreferredLanguage = "es-MX",
            IsGuest = false
        };

        return session;
    }

    public async Task<UserSession> CreateGuestSessionAsync()
    {
        UserSession guestSession = null!;
        string guestCode = _tokenGenerator.GenerateVerificationCode();

        guestSession = new UserSession
        {
            Username = $"Guest_{guestCode}",
            RememberToken = string.Empty,
            PreferredLanguage = "es-MX",
            IsGuest = true
        };

        await Task.CompletedTask;

        return guestSession;
    }

    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        bool isLogoutSuccessful = false;

        if (request is null || string.IsNullOrWhiteSpace(request.SessionToken))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        isLogoutSuccessful = await _tokenInvalidator.InvalidateTokenAsync(request.SessionToken);

        return isLogoutSuccessful;
    }

    private static FaultException<ServiceFault> CreateFault(string errorCode, string messageKey)
    {
        ServiceFault fault = new ServiceFault 
        { 
            ErrorCode = errorCode, 
            MessageKey = messageKey 
        };

        return new FaultException<ServiceFault>(fault, new FaultReason(errorCode));
    }
}