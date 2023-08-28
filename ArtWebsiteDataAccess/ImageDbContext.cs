using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ArtWebsiteDataAccess.Models;

namespace ArtWebsiteDataAccess
{
    public class ImageDbContext : DbContext
    {
        public ImageDbContext(IConfiguration configuration) : base()
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public DbSet<Image> Images { get; set; } // Add a DbSet<Image> property for the Images table

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")!;
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Failed to get the database connection string.");
            }
            optionsBuilder.UseSqlServer();
        }
    }
}
