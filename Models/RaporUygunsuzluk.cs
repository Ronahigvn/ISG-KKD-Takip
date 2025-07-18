/* using ISGKkdTakip.Models; 
public class RaporUygunsuzluk
{
    public int RaporId { get; set; }
    public Rapor Rapor { get; set; }

    public int UygunsuzlukId { get; set; }
    public Uygunsuzluk Uygunsuzluk { get; set; }
}
 */
using Microsoft.AspNetCore.Mvc;
using ISGKkdTakip.Models; // Rapor ve Uygunsuzluk modellerinin olduğu namespace
using ISGKkdTakip.Data; // DbContext burada ise


public class RaporUygunsuzluk
{
    public int RaporId { get; set; }
    public Rapor Rapor { get; set; } = null!; // Uyarıyı susturmak için 'null!' (null-forgiving operator) kullanılır.
                                             // Bu, "ben bunun null olmayacağından eminim" demektir.

    public int UygunsuzlukId { get; set; }
    public Uygunsuzluk Uygunsuzluk { get; set; } = null!; // Aynı şekilde
}