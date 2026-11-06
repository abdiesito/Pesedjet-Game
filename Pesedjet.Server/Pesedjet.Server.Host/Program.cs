using Pesedjet.Server.Contracts.Internal;
using CoreWCF;
using CoreWCF.Configuration;
using Microsoft.EntityFrameworkCore;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.CrossCutting.Security;
using Pesedjet.Server.DataAccess.Contexts;
using Pesedjet.Server.DataAccess.Repositories.Implementations;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;
using Pesedjet.Server.Matchmaking;
using Pesedjet.Server.Matchmaking.Invitations;
using Pesedjet.Server.PlayerManagement.Authentication;
using Pesedjet.Server.PlayerManagement.Friendships;
using Pesedjet.Server.PlayerManagement.Registration;

const string ConnectionStringName = "DefaultConnection";
const string MissingConnectionStringMessage = "ConnectionStrings:DefaultConnection was not found in the configuration.";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

string connectionString = builder.Configuration.GetConnectionString(ConnectionStringName)
    ?? throw new InvalidOperationException(MissingConnectionStringMessage);

builder.WebHost.UseNetTcp(ServiceEndpoints.TcpPort);
builder.Services.AddServiceModelServices();
builder.Services.AddDbContext<PesedjetDatabaseContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<ITokenGenerator, TokenGenerator>();
builder.Services.AddSingleton<IActivePlayers, ActivePlayers>();

builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IFriendshipRepository, FriendshipRepository>();
builder.Services.AddScoped<IBlockedPlayerRepository, BlockedPlayerRepository>();

builder.Services.AddTransient<AccountService>();
builder.Services.AddTransient<AuthServiceDependencies>();
builder.Services.AddTransient<AuthService>();
builder.Services.AddSingleton<IFriendshipNotifier, FriendshipNotifier>();
builder.Services.AddTransient<FriendshipServiceDependencies>();
builder.Services.AddTransient<FriendshipService>();
builder.Services.AddSingleton<FriendshipNotificationService>();
builder.Services.AddSingleton<LobbyService>();
builder.Services.AddSingleton<ILobbyDirectory>(ResolveLobbyDirectory);
builder.Services.AddTransient<LobbyInvitationServiceDependencies>();
builder.Services.AddTransient<LobbyInvitationService>();

WebApplication app = builder.Build();

((IApplicationBuilder)app).UseServiceModel(ConfigureServiceModel);

app.Run();

static ILobbyDirectory ResolveLobbyDirectory(IServiceProvider serviceProvider)
{
    return serviceProvider.GetRequiredService<LobbyService>();
}

static void ConfigureServiceModel(IServiceBuilder serviceBuilder)
{
    NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None);

    serviceBuilder.AddService<AccountService>()
        .AddServiceEndpoint<AccountService, IAccountService>(tcpBinding, ServiceEndpoints.AccountServiceAddress);

    serviceBuilder.AddService<AuthService>()
        .AddServiceEndpoint<AuthService, IAuthService>(tcpBinding, ServiceEndpoints.AuthServiceAddress);

    serviceBuilder.AddService<FriendshipService>()
        .AddServiceEndpoint<FriendshipService, IFriendshipService>(tcpBinding, ServiceEndpoints.FriendshipServiceAddress);

    serviceBuilder.AddService<FriendshipNotificationService>()
        .AddServiceEndpoint<FriendshipNotificationService, IFriendshipNotificationService>(
            tcpBinding,
            ServiceEndpoints.FriendshipNotificationServiceAddress);

    serviceBuilder.AddService<LobbyService>()
        .AddServiceEndpoint<LobbyService, ILobbyService>(tcpBinding, ServiceEndpoints.LobbyServiceAddress);

    serviceBuilder.AddService<LobbyInvitationService>()
        .AddServiceEndpoint<LobbyInvitationService, ILobbyInvitationService>(
            tcpBinding,
            ServiceEndpoints.LobbyInvitationServiceAddress);
}
