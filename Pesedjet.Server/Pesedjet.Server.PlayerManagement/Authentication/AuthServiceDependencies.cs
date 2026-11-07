using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.CrossCutting.Security;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;

namespace Pesedjet.Server.PlayerManagement.Authentication;

public sealed record AuthServiceDependencies(
    IPlayerRepository PlayerRepository,
    IPasswordHasher PasswordHasher,
    ITokenGenerator TokenGenerator,
    IActivePlayers ActivePlayers);
