using System.ComponentModel.DataAnnotations;

namespace ISGKkdTakip.Models
{
    public class Mekan
    {
        public int Id { get; set; }

        [Required]
        public string Ad { get; set; }

        public ICollection<Rapor> Raporlar { get; set; }
    }
}
