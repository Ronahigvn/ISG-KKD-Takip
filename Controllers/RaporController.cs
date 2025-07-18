using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic; // Dictionary için gerekli
using System.Linq;
using ISGKkdTakip.Models; // Mekan modeliniz için
using ISGKkdTakip.Data; // ApplicationDbContext için

namespace ISGKkdTakip.Controllers
{
    public class RaporController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RaporController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Rapor/Index — Rapor listesini ve grafik için verileri göster
        public IActionResult Index()
        {
            // Veritabanından toplam kişi ve ekipman kullanan kişi sayılarını al
            var toplam = _context.Raporlar.Sum(r => r.ToplamKisi);
            var kullanan = _context.Raporlar.Sum(r => r.EkipmanKullanan);
            var kullanmayan = toplam - kullanan;

            // Bu verileri ViewBag aracılığıyla view'a gönder
            ViewBag.TotalPeople = toplam;
            ViewBag.WithKkd = kullanan;
            ViewBag.WithoutKkd = kullanmayan;

            // Tüm raporları al ve view'a model olarak gönder
            var raporlar = _context.Raporlar.ToList();
            return View(raporlar);
            
        }

        // POST: Rapor/CreateFromResult — Result view’dan gelen tahminle kaydet
        [HttpPost]
        public IActionResult CreateFromResult(int toplamKisi, int ekipmanKullanan)
        {
            // Örnek mekan ve uygunsuzluk id’leri (istersen kullanıcıdan veya sabit)
            // Gerçek uygulamada bu ID'ler kullanıcı seçimi veya başka bir mantıkla belirlenmelidir.
            int mekanId = 1; 
            int uygunsuzlukId = 1; 

            var rapor = new Rapor
            {
                ToplamKisi = toplamKisi,
                EkipmanKullanan = ekipmanKullanan,
                // Tarih alanı için varsayılan değer ataması yapılmalı veya modelde required olmamalıdır.
                Tarih = System.DateTime.Now, // Örnek: Mevcut tarihi ata
                MekanId = mekanId,
                UygunsuzlukId = uygunsuzlukId
            };

            _context.Raporlar.Add(rapor);
            _context.SaveChanges(); // Değişiklikleri veritabanına kaydet

            return RedirectToAction("Index"); // Rapor listesi sayfasına geri dön
        }

        // GET: Rapor/Create (Opsiyonel manuel ekleme için)
        public IActionResult Create()
        {
            // Mekan ve Uygunsuzluk listelerini dropdown için view'a gönder
            ViewBag.Mekanlar = new SelectList(_context.Mekanlar, "Id", "Ad");
            ViewBag.Uygunsuzluklar = new SelectList(_context.Uygunsuzluklar, "Id", "Tip");
            return View();
        }

        // POST: Rapor/Create — manuel kayıt için
        [HttpPost]
        public IActionResult Create(Rapor model)
        {
            if (ModelState.IsValid) // Model geçerliliğini kontrol et
            {
                _context.Raporlar.Add(model);
                _context.SaveChanges(); // Değişiklikleri veritabanına kaydet
                return RedirectToAction("Index"); // Rapor listesi sayfasına geri dön
            }

            // Model geçerli değilse, dropdown'ları tekrar doldur ve view'ı tekrar göster
            ViewBag.Mekanlar = new SelectList(_context.Mekanlar, "Id", "Ad", model.MekanId);
            ViewBag.Uygunsuzluklar = new SelectList(_context.Uygunsuzluklar, "Id", "Tip", model.UygunsuzlukId);
            return View(model);
        }
    }
}
