using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISGKkdTakip.Models
{
    public class Mekan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Ad { get; set; }

        public string Aciklama { get; set; }

        // İlişki: Bir mekanın birden fazla raporu olabilir
        public ICollection<Rapor> Raporlar { get; set; }
    }
}