namespace Pesedjet.Server.Contracts.Interfaces;

using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

[ServiceContract]
public interface IIdentityService
{
    [OperationContract]
    LogoutResponse Logout(LogoutRequest request);
}
