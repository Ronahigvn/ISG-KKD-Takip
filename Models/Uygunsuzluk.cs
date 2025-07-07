using System.ComponentModel.DataAnnotations;

namespace ISGKkdTakip.Models
{
    public class Uygunsuzluk
    {
        public int Id { get; set; }

        [Required]
        public string Tip { get; set; } // Örn: Baret Yok, Eldiven Yok

        public ICollection<Rapor> Raporlar { get; set; }
    }
}
