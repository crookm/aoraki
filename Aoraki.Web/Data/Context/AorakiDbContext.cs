using Aoraki.Web.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Aoraki.Web.Data.Context
{
    public partial class AorakiDbContext : DbContext
    {
        public AorakiDbContext() { }
        public AorakiDbContext(DbContextOptions<AorakiDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseNpgsql("Name=Postgres")
                    .UseSnakeCaseNamingConvention();
            }
        }

        public DbSet<JournalPost> JournalPosts { get; set; }
    }
}