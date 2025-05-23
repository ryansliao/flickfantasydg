using fantasydg.Models;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.Data
{
    public class DGDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<PlayerTournament> PlayerTournaments { get; set; }
        public DbSet<RoundScore> RoundScores { get; set; }

        public DGDbContext(DbContextOptions<DGDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tournament>()
                .HasKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<PlayerTournament>()
                .HasKey(pt => new { pt.PlayerId, pt.TournamentId, pt.Division });

            modelBuilder.Entity<PlayerTournament>()
                .HasOne(pt => pt.Tournament)
                .WithMany(t => t.PlayerTournaments)
                .HasForeignKey(pt => new { pt.TournamentId, pt.Division })
                .HasPrincipalKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<Round>()
                .HasKey(r => r.RoundId);

            modelBuilder.Entity<Round>()
                .HasOne(r => r.Tournament)
                .WithMany(t => t.Rounds)
                .HasForeignKey(r => new { r.TournamentId, r.Division })
                .HasPrincipalKey(t => new { t.Id, t.Division });

            modelBuilder.Entity<RoundScore>()
                .HasKey(rs => new { rs.RoundId, rs.PlayerId });

            modelBuilder.Entity<RoundScore>()
                .HasOne(rs => rs.Round)
                .WithMany(r => r.RoundScores)
                .HasForeignKey(rs => rs.RoundId);
        }
    }
}