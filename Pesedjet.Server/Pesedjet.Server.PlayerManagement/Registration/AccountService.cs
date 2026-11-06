using CoreWCF;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.CrossCutting.Security;
using Pesedjet.Server.DataAccess.Entities.Identity;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.PlayerManagement.Registration;

public class AccountService : IAccountService
{
    private const int MinimumAgeInYears = 12;

    private readonly IPlayerRepository _playerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;

    public AccountService(
        IPlayerRepository playerRepository,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider)
    {
        _playerRepository = playerRepository;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
    }

    public bool RegisterAccount(UserRegisterRequest request)
    {
        bool isCreated = false;

        EnsureRequestIsValid(request);
        EnsureAccountIsAvailable(request);

        Player player = CreatePlayer(request);
        isCreated = _playerRepository.CreatePlayer(player);

        return isCreated;
    }

    public bool VerifyCode(string usernameOrEmail, string code, string purpose)
    {
        bool isVerified = false;

        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(code))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        isVerified = _playerRepository.VerifyCode(usernameOrEmail, code, purpose);

        return isVerified;
    }

    public bool RequestPasswordReset(string email)
    {
        bool isProcessed = false;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = _playerRepository.GetPlayerByEmail(email);
        if (player is null)
        {
            throw CreateFault(ServiceErrorCodes.UserNotFound, ServiceMessageKeys.UserNotFound);
        }

        isProcessed = _playerRepository.GeneratePasswordResetCode(player.Id);

        return isProcessed;
    }

    public bool ResetPassword(ResetPasswordRequest request)
    {
        bool isResetSuccessful = false;

        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = _playerRepository.GetPlayerByEmail(request.Email);
        if (player is null)
        {
            throw CreateFault(ServiceErrorCodes.UserNotFound, ServiceMessageKeys.UserNotFound);
        }

        bool isValidCode = _playerRepository.VerifyCode(request.Email, request.Code, "PASSWORD_RESET");
        if (!isValidCode)
        {
            throw CreateFault(ServiceErrorCodes.InvalidCode, ServiceMessageKeys.InvalidCode);
        }

        string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword, player.Id);
        isResetSuccessful = _playerRepository.UpdatePasswordHash(player.Id, newPasswordHash);

        return isResetSuccessful;
    }

    public bool ChangePassword(ChangePasswordRequest request)
    {
        bool isChanged = false;

        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = _playerRepository.GetPlayerByUsername(request.Username);
        if (player is null)
        {
            throw CreateFault(ServiceErrorCodes.UserNotFound, ServiceMessageKeys.UserNotFound);
        }

        bool isCurrentPasswordCorrect = _passwordHasher.VerifyPassword(request.CurrentPassword, player.PasswordHash, player.Id);
        if (!isCurrentPasswordCorrect)
        {
            throw CreateFault(ServiceErrorCodes.InvalidCredentials, ServiceMessageKeys.InvalidCredentials);
        }

        string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword, player.Id);
        isChanged = _playerRepository.UpdatePasswordHash(player.Id, newPasswordHash);

        return isChanged;
    }

    private static FaultException<ServiceFault> CreateFault(string errorCode, string messageKey)
    {
        var fault = new ServiceFault { ErrorCode = errorCode, MessageKey = messageKey };

        return new FaultException<ServiceFault>(fault, new FaultReason(errorCode));
    }

    private void EnsureRequestIsValid(UserRegisterRequest request)
    {
        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        DateOnly birthDate = DateOnly.FromDateTime(request.BirthDate);

        bool hasRequiredFields = !string.IsNullOrWhiteSpace(request.FullName)
                                 && !string.IsNullOrWhiteSpace(request.Username)
                                 && !string.IsNullOrWhiteSpace(request.Email)
                                 && !string.IsNullOrWhiteSpace(request.Password);
        bool hasValidBirthDate = birthDate.AddYears(MinimumAgeInYears) <= today;

        if (!hasRequiredFields || !hasValidBirthDate)
        {
            throw CreateFault(ServiceErrorCodes.InvalidRegistrationData, ServiceMessageKeys.InvalidRegistrationData);
        }
    }

    private void EnsureAccountIsAvailable(UserRegisterRequest request)
    {
        Player? playerWithUsername = _playerRepository.GetPlayerByUsername(request.Username);

        if (playerWithUsername is not null)
        {
            throw CreateFault(
                ServiceErrorCodes.UsernameAlreadyRegistered,
                ServiceMessageKeys.UsernameAlreadyRegistered);
        }

        Player? playerWithEmail = _playerRepository.GetPlayerByEmail(request.Email);

        if (playerWithEmail is not null)
        {
            throw CreateFault(ServiceErrorCodes.EmailAlreadyRegistered, ServiceMessageKeys.EmailAlreadyRegistered);
        }
    }

    private Player CreatePlayer(UserRegisterRequest request)
    {
        Guid playerId = Guid.NewGuid();

        return new Player
        {
            Id = playerId,
            FullName = request.FullName,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password, playerId),
            DateOfBirth = request.BirthDate
        };
    }
}