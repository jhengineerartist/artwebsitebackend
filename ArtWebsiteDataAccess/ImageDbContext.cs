using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ArtWebsiteDataAccess.Models;

namespace ArtWebsiteDataAccess
{
    public class ImageDbContext : DbContext
    {
        public ImageDbContext(DbContextOptions<ImageDbContext> options) : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!;
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Failed to get the database connection string.");
            }
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("ArtWebsiteAPI"));
        }

    }
}
