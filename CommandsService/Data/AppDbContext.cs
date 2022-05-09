using CommandsService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandsService.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt): base(opt)
        {
        }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Command> Commands { get; set; }
        // This method is to explicitely declare the relationship between the two entities(Platform and Command)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the relationship between Platform and Command
            modelBuilder
                .Entity<Platform>()
                .HasMany(p => p.Commands)
                .WithOne(p => p.Platform)
                .HasForeignKey(p => p.PlatformId);

            // Define the relationship between Command and Platform
            modelBuilder
                .Entity<Command>()
                .HasOne(p => p.Platform)
                .WithMany(p => p.Commands)
                .HasForeignKey(p => p.PlatformId);
        }
    }
}