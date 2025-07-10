using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISGKkdTakip.Models
{
    public class Rapor
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Mekan")]
        public int MekanId { get; set; }
        public Mekan Mekan { get; set; }

        [ForeignKey("Uygunsuzluk")]
        public int UygunsuzlukId { get; set; }
        public Uygunsuzluk Uygunsuzluk { get; set; }

        public int ToplamKisi { get; set; }
        public int EkipmanKullanan { get; set; }

        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}
