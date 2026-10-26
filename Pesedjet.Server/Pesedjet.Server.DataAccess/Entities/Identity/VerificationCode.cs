using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pesedjet.Server.DataAccess.Entities.Enums;

namespace Pesedjet.Server.DataAccess.Entities.Identity;

[Table("VerificationCode", Schema = "Identity")]
public class VerificationCode
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public Player Player { get; set; } = null!;

    [Required]
    [MaxLength(6)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public CodePurpose Purpose { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public bool IsUsed { get; set; }
}
