using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces
{
    [ServiceContract]
    public interface IAuthService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UserSession Login(UserLoginRequest request);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UserSession ResumeSession(string rememberToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        UserSession CreateGuestSession();

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool Logout(LogoutRequest request);
    }
}
