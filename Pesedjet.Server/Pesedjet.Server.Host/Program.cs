#nullable enable

using System;
using CoreWCF;
using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.CrossCutting.Security;
using Pesedjet.Server.DataAccess.Contexts;
using Pesedjet.Server.DataAccess.Repositories.Implementations;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;
using Pesedjet.Server.Matchmaking.Services;
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
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddTransient<AccountService>();
builder.Services.AddSingleton<RoomService>();

WebApplication app = builder.Build();

((IApplicationBuilder)app).UseServiceModel(serviceBuilder =>
{
    NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None);

    serviceBuilder.AddService<AccountService>()
        .AddServiceEndpoint<AccountService, IAccountService>(tcpBinding, ServiceEndpoints.AccountServiceAddress);

    serviceBuilder.AddService<RoomService>()
        .AddServiceEndpoint<RoomService, IRoomService>(tcpBinding, ServiceEndpoints.RoomServiceAddress);
});

app.Run();