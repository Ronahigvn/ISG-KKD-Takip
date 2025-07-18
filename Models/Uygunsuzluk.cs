using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISGKkdTakip.Models
{
    public class Uygunsuzluk
    {
        [Key]
        public int Id { get; set; }// Uygunsuzluk türünün benzersiz tanımlayıcısı

        [Required]
        public string Ad { get; set; }// Uygunsuzluk türü

        public string Aciklama { get; set; }// Uygunsuzluk hakkında ek bilgiler veya detaylar 

        // İlişki: Bir uygunsuzluğun birden fazla raporu olabilir
        public ICollection<Rapor> Raporlar { get; set; }
        

public List<RaporUygunsuzluk> RaporUygunsuzluklar { get; set; }
    }
}
