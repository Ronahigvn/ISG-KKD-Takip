using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ISGKkdTakip.Models;
using System.Linq;
using ISGKkdTakip.Data;


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
        var toplam = _context.Raporlar.Sum(r => r.ToplamKisi);
        var kullanan = _context.Raporlar.Sum(r => r.EkipmanKullanan);
        var kullanmayan = toplam - kullanan;

        ViewBag.TotalPeople = toplam;
        ViewBag.WithKkd = kullanan;
        ViewBag.WithoutKkd = kullanmayan;

        var raporlar = _context.Raporlar.ToList();
        return View(raporlar);
    }

    // POST: Rapor/CreateFromResult — Result view’dan gelen tahminle kaydet
    [HttpPost]
    public IActionResult CreateFromResult(int toplamKisi, int ekipmanKullanan)
    {
        // Örnek mekan ve uygunsuzluk id’leri (istersen kullanıcıdan veya sabit)
        int mekanId = 1;
        int uygunsuzlukId = 1;

        var rapor = new Rapor
        {
            ToplamKisi = toplamKisi,
            EkipmanKullanan = ekipmanKullanan,
            MekanId = mekanId,
            UygunsuzlukId = uygunsuzlukId
        };

        _context.Raporlar.Add(rapor);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // GET: Rapor/Create (Opsiyonel manuel ekleme için)
    public IActionResult Create()
    {
        ViewBag.Mekanlar = new SelectList(_context.Mekanlar, "Id", "Ad");
        ViewBag.Uygunsuzluklar = new SelectList(_context.Uygunsuzluklar, "Id", "Tip");
        return View();
    }

    // POST: Rapor/Create — manuel kayıt için
    [HttpPost]
    public IActionResult Create(Rapor model)
    {
        if (ModelState.IsValid)
        {
            _context.Raporlar.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        ViewBag.Mekanlar = new SelectList(_context.Mekanlar, "Id", "Ad", model.MekanId);
        ViewBag.Uygunsuzluklar = new SelectList(_context.Uygunsuzluklar, "Id", "Tip", model.UygunsuzlukId);
        return View(model);
    }
}
