namespace Pesedjet.Server.Contracts.Interfaces;

using System.ServiceModel;
using System.Threading.Tasks;
using Pesedjet.Server.Contracts.DTOs;

[ServiceContract]
public interface IIdentityService
{
    [OperationContract]
    Task<LogoutResponse> LogoutAsync(LogoutRequest request);
}