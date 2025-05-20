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
            modelBuilder.Entity<Tournament>()
                .HasMany(t => t.RoundList)
                .WithOne(r => r.Tournament)
                .HasForeignKey(r => r.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Round>()
                .HasMany(r => r.Players)
                .WithOne(p => p.Round)
                .HasForeignKey(p => p.RoundId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}