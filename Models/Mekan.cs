using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISGKkdTakip.Models
{
    /// Uygulamadaki fiziksel mekanları (örneğin, şantiyeler, depolar, ofisler) temsil eden model sınıfı.
    public class Mekan
    {
        [Key]
        public int Id { get; set; }// Mekanın benzersiz tanımlayıcısıdır.

        [Required]
        public string Ad { get; set; }  // Mekanın adını temsil eder

        public string Aciklama { get; set; }// Mekan hakkında ek bilgiler veya detaylar için kullanılır.

        
        public ICollection<Rapor> Raporlar { get; set; }// Bu mekanla ilişkili tüm raporları içeren bir koleksiyondur.İlişki: Bir mekanın birden fazla raporu olabilir
    }
}