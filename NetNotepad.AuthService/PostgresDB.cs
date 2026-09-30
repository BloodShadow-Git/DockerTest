using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetNotepad.ServiceBase;

namespace NetNotepad.AuthService
{
    public class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshTokenData> RefreshTokens { get; set; } = null!;
        public AppDBContext() { }
        public AppDBContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { if (!optionsBuilder.IsConfigured) { optionsBuilder.UseNpgsql(PostgresDBConnection.DBConnString); } }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.UserGuid);
            modelBuilder.Entity<User>().HasIndex(u => u.UserGuid);
            modelBuilder.Entity<RefreshTokenData>().HasKey(u => u.RefreshToken);
            modelBuilder.Entity<RefreshTokenData>().HasIndex(u => u.UserGuid);
            modelBuilder.Entity<RefreshTokenData>().HasOne<User>().WithMany().HasForeignKey(u => u.UserGuid).IsRequired().OnDelete(DeleteBehavior.Cascade);
        }
    }

    public record User(Guid UserGuid, string UserLogin, string PasswordHash, TimeSpan RefreshTokenTTL);
    public record RefreshTokenData(Guid UserGuid, string RefreshToken, DateTime ExpireDate, string DeviceName, bool Persistent);

    public class AppDBContextFactory : IDesignTimeDbContextFactory<AppDBContext>
    {
        public AppDBContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDBContext>();

            options.UseNpgsql(
                "Host=localhost;Database=build;Username=build;Password=build");

            return new AppDBContext(options.Options);
        }
    }
}