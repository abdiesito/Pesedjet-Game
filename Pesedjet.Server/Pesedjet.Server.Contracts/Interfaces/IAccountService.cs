using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces
{
    [ServiceContract]
    public interface IAccountService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<bool> RegisterAccountAsync(UserRegisterRequest request);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<bool> VerifyCodeAsync(string usernameOrEmail, string code, string purpose);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        Task<bool> RequestPasswordResetAsync(string email);

        //[OperationContract]
        //[FaultContract(typeof(ServiceFault))]
        //Task<bool> ResetToNewPasswordAsync(ResetPassword request);

        //[OperationContract]
        //[FaultContract(typeof(ServiceFault))]
        //Task<bool> ChangePasswordAsync(ChangePassword request);
    }
}