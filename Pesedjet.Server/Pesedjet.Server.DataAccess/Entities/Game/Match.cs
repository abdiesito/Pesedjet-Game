using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pesedjet.Server.DataAccess.Entities.Game;

[Table("Match", Schema = "Game")]
public class Match
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public DateTime PlayedAt { get; set; }

    [Required]
    public int PlayerCount { get; set; } // RJ-02

    public ICollection<MatchResult> Results { get; set; } = new List<MatchResult>();
}