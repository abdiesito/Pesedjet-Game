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
    [ForeignKey("PlayerId")]
    public Player Player { get; set; }

    [Required, MaxLength(256)]
    public string TokenHash { get; set; } 

    [Required, MaxLength(100)]
    public string Device { get; set; }

    [Required]
    public DateTime ExpiresAt { get; set; } 
}