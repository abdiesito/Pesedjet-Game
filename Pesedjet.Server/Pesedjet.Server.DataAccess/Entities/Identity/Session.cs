using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pesedjet.Server.DataAccess.Entities.Identity;

[Table("Session", Schema = "Identity")]
public class Session
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public Player Player { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Device { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }
}
