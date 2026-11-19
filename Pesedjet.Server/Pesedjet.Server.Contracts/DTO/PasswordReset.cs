using System.Runtime.Serialization;

namespace Pesedjet.Server.Contracts.DTO
{
    [DataContract]
    public class PasswordReset
    {
        [DataMember] public string Email { get; set; } = string.Empty;
        [DataMember] public string NewPassword { get; set; } = string.Empty;
        [DataMember] public string Code { get; set; } = string.Empty;
    }
}