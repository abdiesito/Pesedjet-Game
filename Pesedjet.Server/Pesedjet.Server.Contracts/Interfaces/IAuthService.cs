using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces
{
    [ServiceContract]
    public interface IAuthService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<UserSession> LoginAsync(UserLoginRequest request);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<UserSession> ResumeSessionAsync(string rememberToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<UserSession> CreateGuestSessionAsync();

        [OperationContract(IsOneWay = true)]
        Task LogoutAsync(string username);
    }
}