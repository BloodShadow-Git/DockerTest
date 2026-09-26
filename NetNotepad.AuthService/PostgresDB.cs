using Microsoft.EntityFrameworkCore;

namespace NetNotepad.AuthService
{
    public class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshTokenData> RefreshTokens { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(Program.PostgresConnectString)) { optionsBuilder.UseNpgsql(Program.PostgresConnectString); }
            else { optionsBuilder.UseNpgsql("build"); }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => new { u.UserGuid });
            modelBuilder.Entity<RefreshTokenData>().HasKey(u => new { u.RefreshToken });
        }
    }

    public record User(Guid UserGuid, string UserName, string PasswordHash);
    public record RefreshTokenData(Guid UserGuid, string RefreshToken, DateTime LastUseDate, TimeSpan Ttl);
}