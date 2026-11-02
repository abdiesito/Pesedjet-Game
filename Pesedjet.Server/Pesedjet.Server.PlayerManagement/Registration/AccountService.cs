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

    public async Task<bool> RegisterAccountAsync(UserRegisterRequest request)
    {
        bool isCreated = false;

        EnsureRequestIsValid(request);
        await EnsureAccountIsAvailableAsync(request);

        Player player = CreatePlayer(request);
        isCreated = await _playerRepository.CreatePlayerAsync(player);

        return isCreated;
    }

    public async Task<bool> VerifyCodeAsync(string usernameOrEmail, string code, string purpose)
    {
        bool isVerified = false;

        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(code))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        isVerified = await _playerRepository.VerifyCodeAsync(usernameOrEmail, code, purpose);

        return isVerified;
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        bool isProcessed = false;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = await _playerRepository.GetPlayerByEmailAsync(email);
        if (player is null)
        {
            throw CreateFault(ServiceErrorCodes.UserNotFound, ServiceMessageKeys.UserNotFound);
        }

        isProcessed = await _playerRepository.GeneratePasswordResetCodeAsync(player.Id);

        return isProcessed;
    }

    public async Task<bool> ResetToNewPasswordAsync(ResetPasswordRequest request)
    {
        bool isResetSuccessful = false;

        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = await _playerRepository.GetPlayerByEmailAsync(request.Email);
        if (player is null)
        {
            throw CreateFault(ServiceErrorCodes.UserNotFound, ServiceMessageKeys.UserNotFound);
        }

        bool isValidCode = await _playerRepository.VerifyCodeAsync(request.Email, request.Code, "PASSWORD_RESET");
        if (!isValidCode)
        {
            throw CreateFault(ServiceErrorCodes.InvalidCode, ServiceMessageKeys.InvalidCode);
        }

        string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword, player.Id);
        isResetSuccessful = await _playerRepository.UpdatePasswordHashAsync(player.Id, newPasswordHash);

        return isResetSuccessful;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        bool isChanged = false;

        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw CreateFault(ServiceErrorCodes.InvalidData, ServiceMessageKeys.InvalidData);
        }

        Player? player = await _playerRepository.GetPlayerByUsernameAsync(request.Username);
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
        isChanged = await _playerRepository.UpdatePasswordHashAsync(player.Id, newPasswordHash);

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

    private async Task EnsureAccountIsAvailableAsync(UserRegisterRequest request)
    {
        Player? playerWithUsername = await _playerRepository.GetPlayerByUsernameAsync(request.Username);

        if (playerWithUsername is not null)
        {
            throw CreateFault(
                ServiceErrorCodes.UsernameAlreadyRegistered,
                ServiceMessageKeys.UsernameAlreadyRegistered);
        }

        Player? playerWithEmail = await _playerRepository.GetPlayerByEmailAsync(request.Email);

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