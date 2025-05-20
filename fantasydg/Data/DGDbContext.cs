using fantasydg.Models;
using Microsoft.EntityFrameworkCore;

namespace fantasydg.Data
{
    public class DGDbContext : DbContext
    {
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Player> Players { get; set; }

        public DGDbContext(DbContextOptions<DGDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite primary key for Tournament
            modelBuilder.Entity<Tournament>()
                .HasKey(t => new { t.Id, t.Division });

            // Round references Tournament by both Id and Division
            modelBuilder.Entity<Round>()
                .HasOne(r => r.Tournament)
                .WithMany(t => t.RoundList)
                .HasForeignKey(r => new { r.TournamentId, r.Division })
                .OnDelete(DeleteBehavior.Cascade);

            // Player references Round as before
            modelBuilder.Entity<Round>()
                .HasMany(r => r.Players)
                .WithOne(p => p.Round)
                .HasForeignKey(p => p.RoundId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}