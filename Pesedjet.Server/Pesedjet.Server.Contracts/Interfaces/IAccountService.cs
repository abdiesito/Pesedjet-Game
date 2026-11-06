using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces
{
    [ServiceContract]
    public interface IAccountService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool RegisterAccount(UserRegisterRequest request);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool VerifyCode(string usernameOrEmail, string code, string purpose);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool RequestPasswordReset(string email);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool ResetPassword(ResetPasswordRequest request);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        bool ChangePassword(ChangePasswordRequest request);
    }
}
