using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetNotepad.AuthService
{
    public class AppDBContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshTokenData> RefreshTokens { get; set; } = null!;
        public AppDBContext() => Migrate();
        public AppDBContext(DbContextOptions options) : base(options) => Migrate();

        private void Migrate() { if (!EF.IsDesignTime) { Database.Migrate(); } }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { if (!optionsBuilder.IsConfigured) { optionsBuilder.UseNpgsql(Program.PostgresConnectString); } }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => new { u.UserGuid });
            modelBuilder.Entity<RefreshTokenData>().HasKey(u => new { u.RefreshToken });
        }
    }

    public record User(Guid UserGuid, string UserName, string PasswordHash);
    public record RefreshTokenData(Guid UserGuid, string RefreshToken, DateTime LastUseDate, TimeSpan Ttl);

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