using ISGKkdTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace ISGKkdTakip.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Mekan> Mekanlar { get; set; }
        public DbSet<Uygunsuzluk> Uygunsuzluklar { get; set; }
        public DbSet<Rapor> Raporlar { get; set; }

        public DbSet<RaporUygunsuzluk> RaporUygunsuzluklar { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<RaporUygunsuzluk>()
        .HasKey(ru => new { ru.RaporId, ru.UygunsuzlukId });

    modelBuilder.Entity<RaporUygunsuzluk>()
        .HasOne(ru => ru.Rapor)
        .WithMany(r => r.RaporUygunsuzluklar)
        .HasForeignKey(ru => ru.RaporId);

    modelBuilder.Entity<RaporUygunsuzluk>()
        .HasOne(ru => ru.Uygunsuzluk)
        .WithMany()
        .HasForeignKey(ru => ru.UygunsuzlukId);
}



    }
}
