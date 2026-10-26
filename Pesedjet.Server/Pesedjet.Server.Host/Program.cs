using CoreWCF;
using CoreWCF.Configuration;
using Microsoft.EntityFrameworkCore;
using Pesedjet.Server.Contracts.Endpoints;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.CrossCutting.Security;
using Pesedjet.Server.DataAccess.Contexts;
using Pesedjet.Server.DataAccess.Repositories.Implementations;
using Pesedjet.Server.DataAccess.Repositories.Interfaces;
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

WebApplication app = builder.Build();

((IApplicationBuilder)app).UseServiceModel(serviceBuilder =>
{
    var tcpBinding = new NetTcpBinding(SecurityMode.None);

    serviceBuilder.AddService<AccountService>()
        .AddServiceEndpoint<AccountService, IAccountService>
            (tcpBinding, ServiceEndpoints.AccountServiceAddress);
});

app.Run();
