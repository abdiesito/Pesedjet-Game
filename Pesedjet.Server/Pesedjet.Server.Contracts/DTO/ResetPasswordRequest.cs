using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTOs
{
    [DataContract]
    public class ResetPasswordRequest
    {
        [DataMember] public string Email { get; set; } = string.Empty;
        [DataMember] public string NewPassword { get; set; } = string.Empty;
        [DataMember] public string Code { get; set; } = string.Empty;
    }
}