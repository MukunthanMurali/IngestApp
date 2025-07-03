using IngesWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IngesWebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Video> HomepageTopVideos { get; set; }
    }
}
