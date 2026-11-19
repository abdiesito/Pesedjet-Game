using Pesedjet.Server.Contracts.DTO;

namespace Pesedjet.Server.Contracts.Interfaces;

using System.ServiceModel;

[ServiceContract]
public interface IIdentityService
{
    [OperationContract]
    LogoutResponse Logout(LogoutRequest request);
}
