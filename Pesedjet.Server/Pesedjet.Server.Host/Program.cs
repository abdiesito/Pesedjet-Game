using CoreWCF;
using CoreWCF.Configuration;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.Identity.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseNetTcp(8081);
builder.Services.AddServiceModelServices();
builder.Services.AddTransient<AuthManagerService>();

var app = builder.Build();

((IApplicationBuilder)app).UseServiceModel(serviceBuilder =>
{
    var tcpBinding = new NetTcpBinding(SecurityMode.None);

    serviceBuilder.AddService<AuthManagerService>()
        .AddServiceEndpoint<AuthManagerService, IAuthManager>
            (tcpBinding, "net.tcp://localhost:8081/AuthService");
});

app.Run();