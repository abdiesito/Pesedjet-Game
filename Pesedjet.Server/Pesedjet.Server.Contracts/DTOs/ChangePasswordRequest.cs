using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs
{
    [DataContract]
    public class ChangePasswordRequest
    {
        [DataMember] 
        public string Username { get; set; } = string.Empty;
        
        [DataMember] 
        public string CurrentPassword { get; set; } = string.Empty;
        
        [DataMember] 
        public string NewPassword { get; set; } = string.Empty;
    }
}