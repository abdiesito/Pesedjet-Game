using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces;

[ServiceContract]
public interface IAccountService
{
    [OperationContract]
    [FaultContract(typeof(ServiceFault))]
    Task<bool> RegisterAccountAsync(UserRegisterRequest request);
}
