using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISGKkdTakip.Models
{
    public class Rapor
    {
        // KKD (Kişisel Koruyucu Donanım) tespit sonuçlarını ve ilgili bilgileri saklayan rapor model sınıfı.
        [Key]// Raporun benzersiz tanımlayıcısıdır.
        public int Id { get; set; }
        // Raporun ilişkili olduğu mekanın yabancı anahtarıdır.
        [ForeignKey("Mekan")]
        public int MekanId { get; set; }
        public Mekan Mekan { get; set; }// Raporun ilişkili olduğu Mekan nesnesidir 

        // Raporun ilişkili olduğu uygunsuzluk türünün yabancı anahtarıdır.
        [ForeignKey("Uygunsuzluk")]
        public int UygunsuzlukId { get; set; } // Raporun ilişkili olduğu Uygunsuzluk nesnesi
        public Uygunsuzluk Uygunsuzluk { get; set; }

        public int ToplamKisi { get; set; }// Görseldeki toplam kişi sayısı
        public int EkipmanKullanan { get; set; }// Görseldeki ekipman (KKD) kullanan kişi sayısını belirtir.
[DataType(DataType.Date)]
       public DateTime Tarih { get; set; } = DateTime.UtcNow;// Raporun oluşturulduğu tarihi ve saati belirtir.

    
public List<RaporUygunsuzluk> RaporUygunsuzluklar { get; set; }



    }
}
