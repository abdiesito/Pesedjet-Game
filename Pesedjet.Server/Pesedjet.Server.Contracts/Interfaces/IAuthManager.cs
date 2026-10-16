using System.ServiceModel;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Interfaces;

[ServiceContract]
public interface IAuthManager
{ 
    [OperationContract] 
    [FaultContract(typeof(ServiceFaultDto))] 
    bool RegisterAccount(UserRegisterDto userRegisterDto);
    
    
}

