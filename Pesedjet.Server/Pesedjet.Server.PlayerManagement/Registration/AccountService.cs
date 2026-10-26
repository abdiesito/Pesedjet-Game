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
        EnsureRequestIsValid(request);
        await EnsureAccountIsAvailableAsync(request);

        Player player = CreatePlayer(request);

        return await _playerRepository.CreatePlayerAsync(player);
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
