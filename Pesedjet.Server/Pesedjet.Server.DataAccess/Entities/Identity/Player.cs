using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pesedjet.Server.DataAccess.Entities.Identity;

[Table("Player", Schema = "Identity")]
public class Player
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? ProfileDescription { get; set; }

    [Required]
    public bool IsGuest { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    [Required]
    public int FailedLoginAttempts { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<Game.MatchResult> MatchResults { get; set; } = new List<Game.MatchResult>();
}