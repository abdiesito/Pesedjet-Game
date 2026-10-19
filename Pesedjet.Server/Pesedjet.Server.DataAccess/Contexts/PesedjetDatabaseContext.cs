using Microsoft.EntityFrameworkCore;
using Pesedjet.Server.DataAccess.Entities.Game;
using Pesedjet.Server.DataAccess.Entities.Identity;

namespace Pesedjet.Server.DataAccess.Contexts;

public class PesedjetDatabaseContext : DbContext
{
    public PesedjetDatabaseContext(DbContextOptions<PesedjetDatabaseContext> options) : base(options) { }
    
    public DbSet<Player> Players { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    public DbSet<BlockedPlayer> BlockedUsers { get; set; }

    public DbSet<Match> Matches { get; set; }
    public DbSet<MatchResult> MatchResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Friendship>().HasKey(f => new { f.PlayerRequesterId, f.PlayerAddresseeId });
        
        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.PlayerRequester)
            .WithMany()
            .HasForeignKey(f => f.PlayerRequesterId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Friendship>()
            .HasOne(f => f.PlayerAddressee)
            .WithMany()
            .HasForeignKey(f => f.PlayerAddresseeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<BlockedPlayer>().HasKey(b => new { b.PlayerId, b.BlockedId });
        modelBuilder.Entity<BlockedPlayer>()
            .HasOne(b => b.Player)
            .WithMany()
            .HasForeignKey(b => b.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BlockedPlayer>()
            .HasOne(b => b.Blocked)
            .WithMany()
            .HasForeignKey(b => b.BlockedId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<VerificationCode>()
            .HasOne(v => v.Player)
            .WithMany(u => u.VerificationCodes)
            .HasForeignKey(v => v.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Session>()
            .HasOne(s => s.Player)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<MatchResult>()
            .HasOne(mr => mr.Player)
            .WithMany(u => u.MatchResults)
            .HasForeignKey(mr => mr.PlayerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
    
}