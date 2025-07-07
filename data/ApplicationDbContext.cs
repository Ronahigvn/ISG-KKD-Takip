using Microsoft.EntityFrameworkCore;
using ISGKkdTakip.Models;

namespace ISGKkdTakip.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Mekan> Mekanlar { get; set; }
        public DbSet<Uygunsuzluk> Uygunsuzluklar { get; set; }
        public DbSet<Rapor> Raporlar { get; set; }
    }
}
