/* using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using ISGKkdTakip.Models;
using ISGKkdTakip.Data;
using ISGKkdTakip.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISGKkdTakip.Controllers
{
    // Controller sınıfı, HTTP isteklerini işler ve uygun yanıtları döndürür
    public class ImagePredictionController : Controller
    {
        private readonly ILogger<ImagePredictionController> _logger;// Loglama arayüzü
        private readonly ApplicationDbContext _context;// EF Core

        // Yapıcı metot (Constructor): Bağımlılık Enjeksiyonu ile ILogger ve ApplicationDbContext'i alır  
        public ImagePredictionController(ILogger<ImagePredictionController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        // --- GET İsteği: Görsel Yükleme Sayfasını Görüntüleme ---
        [HttpGet]
        public IActionResult Index()
        {   // Veritabanından Mekanları alır ve SelectListItem'lere dönüştürür
            // Bu liste, View'daki dropdown (açılır liste) için kullanılır
            var mekanlar = _context.Mekanlar
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
                .ToList();

            var viewModel = new ImageUploadViewModel
            {
                MekanList = mekanlar
            };

            return View(viewModel);
        }
        // --- POST İsteği: Görseli Analiz Etme ---
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(ImageUploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ViewBag.ErrorMessage = "Lütfen analiz etmek için bir görsel seçin.";

                model.MekanList = _context.Mekanlar
                    .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
                    .ToList();

                return View("Index", model);
            }
            // --- Dosya Yolu Ayarları ---
            // Yüklenen ve işlenen görsellerin kaydedileceği klasör yolunu belirler
            // wwwroot/uploads klasörünü kullanır
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            // Eğer uploads klasörü yoksa oluşturur
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            // Giriş ve çıkış görsel dosyalarının adlarını belirler
            string inputFileName = "uploaded_image.jpg";
            string outputFileName = "annotated_output.jpg";
            // Giriş ve çıkış görsellerinin tam yollarını oluşturur
            string inputImagePath = Path.Combine(uploadsFolder, inputFileName);
            string outputImagePath = Path.Combine(uploadsFolder, outputFileName);
            // --- Geçici Dosyaları Silme ---
            // Önceki yüklemelerden kalmış olabilecek eski dosyaları silmeye çalışır
            try
            {
                if (System.IO.File.Exists(inputImagePath))
                    System.IO.File.Delete(inputImagePath);
                if (System.IO.File.Exists(outputImagePath))
                    System.IO.File.Delete(outputImagePath);
            }
            catch (IOException ex)
            {
                _logger.LogError($"Dosya silme hatası: {ex.Message}");
                ViewBag.ErrorMessage = "Geçici dosyalar silinemedi.";
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
            // --- Yüklenen Görseli Kaydetme ---
            // Yüklenen görseli sunucudaki inputImagePath'e asenkron olarak kopyalar
            using (var stream = new FileStream(inputImagePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }
            // --- Python Betiğini Çalıştırma ---
            // Python betiğini çalıştırmak için ProcessStartInfo nesnesi oluşturur
            var psi = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"draw_predictions.py \"{inputImagePath}\" \"{outputImagePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            string pythonOutput;// Python betiğinin standart çıktısını tutacak değişken
            string pythonError;  // Python betiğinin hata çıktısını tutacak değişken
            
            // Python betiğini başlatır ve çıktılarını yakalar
            using (var process = Process.Start(psi))
            {
                pythonOutput = await process.StandardOutput.ReadToEndAsync();
                pythonError = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                // --- Python Betiği Sonuçlarını İşleme ---
                // Eğer Python betiği başarıyla çalıştıysa (çıkış kodu 0)
                if (process.ExitCode == 0)
                {   // İşlenmiş görselin web'den erişilebilir yolunu ViewBag'e atar
                    ViewBag.ProcessedImagePath = $"/uploads/{outputFileName}";
                    // KKD sayımlarını tutacak bir sözlük oluşturur
                    var ppeCounts = new Dictionary<string, int>
                    {
                        {"hardhat", 0},
                        {"safety vest", 0},
                        {"goggles", 0},
                        {"mask", 0},
                        {"person", 0} // Kişi sayımı
                    };
                    // Python çıktısını satırlara böler
                    var lines = pythonOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    int totalPeopleDetected = 0; // Toplam kişi sayısını tutacak değişken
                    foreach (var line in lines)
                    {
                        if (line.Contains(":"))
                        {
                            var parts = line.Split(':');
                            if (parts.Length == 2)
                            {
                                string ppeType = parts[0].Trim().ToLower().Replace(" ", "");
                                if (ppeType == "safetyvest") ppeType = "safety vest";

                                if (int.TryParse(parts[1].Trim(), out int count))
                                {
                                    if (ppeCounts.ContainsKey(ppeType))
                                    {
                                        ppeCounts[ppeType] = count;
                                    }
                                    // Eğer çıktı "Person" sayısını içeriyorsa, onu ayır
                                    if (ppeType == "person")
                                    {
                                        totalPeopleDetected = count;
                                    }
                                }
                            }
                        }
                    }

                    // Toplam kişi sayısını Python çıktısından alınan "person" sayısına ata
                    ViewBag.TotalPeople = totalPeopleDetected;





                    // Her bir KKD sayısını ViewBag'e atar (View'da kullanılmak üzere)
                    ViewBag.Hardhats = ppeCounts["hardhat"];
                    ViewBag.Vests = ppeCounts["safety vest"];
                    ViewBag.Goggles = ppeCounts["goggles"];
                    ViewBag.Masks = ppeCounts["mask"];
                    ViewBag.TotalPeople = totalPeopleDetected; 

                     // --- Veritabanına Rapor Kaydetme ---
                    // Yeni bir Rapor nesnesi oluşturur ve verileri atar
                    var rapor = new Rapor
                    {
                        MekanId = model.SelectedMekanId,
                        UygunsuzlukId = 1,
                        ToplamKisi = totalPeopleDetected,
                        EkipmanKullanan = ppeCounts["hardhat"] + ppeCounts["safety vest"] + ppeCounts["goggles"] + ppeCounts["mask"],
                        Tarih = DateTime.UtcNow  // UTC olarak değiştirildi
                    };

                     // Raporu veritabanı bağlamına ekler
                    _context.Raporlar.Add(rapor);
                    // Değişiklikleri veritabanına kaydeder
                    await _context.SaveChangesAsync();
                    /// Sonuçları göstermek için Result View'ını döndürür
                    return View("~/Views/Rapor/Result.cshtml");
                }
                else
                {
                     // --- Python Betiği Hata Durumu ---
                    // Eğer Python betiği hata koduyla çıkarsa, hata çıktısını loglar ve hata View'ını döndürür
                    _logger.LogError($"Python betiği hata verdi: {pythonError}");
                    return View("Error", new ErrorViewModel { RequestId = pythonError });
                }
            }
        }
    }
}
 */
/* 


 using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using ISGKkdTakip.Models;
using ISGKkdTakip.Data;
using ISGKkdTakip.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System; // DateTime için
using System.Linq; // Select, ToList için
using System.Text.Json; // 🎉 JSON işlemleri için eklendi

namespace ISGKkdTakip.Controllers
{
    // 🎉 Python'dan gelen JSON çıktısını temsil eden yardımcı sınıf
    public class PythonOutput
    {
        public string Output_image_path { get; set; }
        public Dictionary<string, int> Ppe_counts { get; set; }
        public int Total_person_count { get; set; }
    }

    // Controller sınıfı, HTTP isteklerini işler ve uygun yanıtları döndürür
    public class ImagePredictionController : Controller
    {
        private readonly ILogger<ImagePredictionController> _logger; // Loglama arayüzü
        private readonly ApplicationDbContext _context; // EF Core

        // Yapıcı metot (Constructor): Bağımlılık Enjeksiyonu ile ILogger ve ApplicationDbContext'i alır
        public ImagePredictionController(ILogger<ImagePredictionController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
public IActionResult Kiyasla(int mekanId)
    {
        var raporlar = _context.Raporlar
            .Where(r => r.MekanId == mekanId)
            .OrderByDescending(r => r.Tarih)
            .Take(2)
            .ToList();

        if (raporlar.Count < 2)
        {
            ViewBag.Mesaj = "Karşılaştırma için en az 2 rapor olmalıdır.";
            return View();
        }

        var bugun = raporlar[0];
        var onceki = raporlar[1];

        var farklar = new Dictionary<string, int>
        {
            { "Kask", bugun.EkipmanKullanan - onceki.EkipmanKullanan }, // İstersen daha detaylı ayırabiliriz
            { "ToplamKisi", bugun.ToplamKisi - onceki.ToplamKisi }
        };

        ViewBag.Farklar = farklar;
        ViewBag.MekanAdi = bugun.Mekan.Ad;
        return View();
    }
        // --- GET İsteği: Görsel Yükleme Sayfasını Görüntüleme ---
        [HttpGet]
        public IActionResult Index()
        {
            // Veritabanından Mekanları alır ve SelectListItem'lere dönüştürür
            // Bu liste, View'daki dropdown (açılır liste) için kullanılır
            var mekanlar = _context.Mekanlar
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
                .ToList();

            var viewModel = new ImageUploadViewModel
            {
                MekanList = mekanlar
            };
 ViewBag.Mekanlar = mekanlar; 
            return View(viewModel);
        }

        // --- POST İsteği: Görseli Analiz Etme ---
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(ImageUploadViewModel model)
        {
            await _context.SaveChangesAsync();

// Mekanlar dropdown için ViewBag'e eklendi
ViewBag.Mekanlar = _context.Mekanlar
    .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
    .ToList();

// Sonuç sayfasına geç
return View("~/Views/Rapor/Result.cshtml");

            if (model.File == null || model.File.Length == 0)
            {
                ViewBag.ErrorMessage = "Lütfen analiz etmek için bir görsel seçin.";

                model.MekanList = _context.Mekanlar
                    .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
                    .ToList();

                return View("Index", model);
            }

            // --- Dosya Yolu Ayarları ---
            // Yüklenen ve işlenen görsellerin kaydedileceği klasör yolunu belirler
            // wwwroot/uploads klasörünü kullanır
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // Eğer uploads klasörü yoksa oluşturur
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Giriş ve çıkış görsel dosyalarının adlarını belirler
            string inputFileName = "uploaded_image.jpg";
            string outputFileName = "annotated_output.jpg";

            // Giriş ve çıkış görsellerinin tam yollarını oluşturur
            string inputImagePath = Path.Combine(uploadsFolder, inputFileName);
            string outputImagePath = Path.Combine(uploadsFolder, outputFileName);

            // --- Geçici Dosyaları Silme ---
            // Önceki yüklemelerden kalmış olabilecek eski dosyaları silmeye çalışır
            try
            {
                if (System.IO.File.Exists(inputImagePath))
                    System.IO.File.Delete(inputImagePath);
                if (System.IO.File.Exists(outputImagePath))
                    System.IO.File.Delete(outputImagePath);
            }
            catch (IOException ex)
            {
                _logger.LogError($"Dosya silme hatası: {ex.Message}");
                ViewBag.ErrorMessage = "Geçici dosyalar silinemedi.";
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }

            // --- Yüklenen Görseli Kaydetme ---
            // Yüklenen görseli sunucudaki inputImagePath'e asenkron olarak kopyalar
            using (var stream = new FileStream(inputImagePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            // --- Python Betiğini Çalıştırma ---
            // Python betiğini çalıştırmak için ProcessStartInfo nesnesi oluşturur
            var psi = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"draw_predictions.py \"{inputImagePath}\" \"{outputImagePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            string pythonOutput; // Python betiğinin standart çıktısını tutacak değişken
            string pythonError;  // Python betiğinin hata çıktısını tutacak değişken

            // Python betiğini başlatır ve çıktılarını yakalar
            using (var process = Process.Start(psi))
            {
                pythonOutput = await process.StandardOutput.ReadToEndAsync();
                pythonError = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                // --- Python Betiği Sonuçlarını İşleme ---
                // Eğer Python betiği başarıyla çalıştıysa (çıkış kodu 0)
                if (process.ExitCode == 0)
                {
                    // 🎉 Python çıktısını JSON olarak ayrıştır
                    PythonOutput parsedOutput = null;
                    try
                    {
                        // En son JSON çıktısını almak için, çıktıdaki son JSON satırını bulmaya çalış
                        // Python betiği birden fazla print yapabilir, bu yüzden son JSON'ı almalıyız.
                        var lastJsonLine = pythonOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .LastOrDefault(line => line.Trim().StartsWith("{") && line.Trim().EndsWith("}"));
                        
                        if (!string.IsNullOrEmpty(lastJsonLine))
                        {
                            parsedOutput = JsonSerializer.Deserialize<PythonOutput>(lastJsonLine, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Python çıktısı JSON ayrıştırma hatası: {ex.Message}\nÇıktı:\n{pythonOutput}");
                        return View("Error", new ErrorViewModel { RequestId = "JSON ayrıştırma hatası." });
                    }

                    if (parsedOutput == null || parsedOutput.Ppe_counts == null)
                    {
                        _logger.LogError($"Python çıktısı boş veya beklenmeyen formatta: {pythonOutput}");
                        return View("Error", new ErrorViewModel { RequestId = "Python çıktısı geçersiz." });
                    }

                    // İşlenmiş görselin web'den erişilebilir yolunu ViewBag'e atar
                    ViewBag.ProcessedImagePath = $"/uploads/{outputFileName}";

                    // 🎉 JSON'dan gelen KKD sayımlarını ViewBag'e aktar
                    ViewBag.Hardhats = parsedOutput.Ppe_counts.GetValueOrDefault("hardhat", 0);
                    ViewBag.Vests = parsedOutput.Ppe_counts.GetValueOrDefault("safety vest", 0);
                    ViewBag.Goggles = parsedOutput.Ppe_counts.GetValueOrDefault("goggles", 0);
                    ViewBag.Masks = parsedOutput.Ppe_counts.GetValueOrDefault("mask", 0);
                    
                    // 🎉 JSON'dan gelen toplam kişi sayısını ViewBag'e aktar
                    ViewBag.TotalPeople = parsedOutput.Total_person_count;

                    // --- Veritabanına Rapor Kaydetme ---
                    // Yeni bir Rapor nesnesi oluşturur ve verileri atar
                    var rapor = new Rapor
                    {
                        MekanId = model.SelectedMekanId,
                        UygunsuzlukId = 1, // Sabit bir uygunsuzluk ID'si (uygulamanıza göre değişebilir)
                        ToplamKisi = parsedOutput.Total_person_count, // 🎉 JSON'dan gelen toplam kişi sayısı
                        EkipmanKullanan = parsedOutput.Ppe_counts.GetValueOrDefault("hardhat", 0) +
                                          parsedOutput.Ppe_counts.GetValueOrDefault("safety vest", 0) +
                                          parsedOutput.Ppe_counts.GetValueOrDefault("goggles", 0) +
                                          parsedOutput.Ppe_counts.GetValueOrDefault("mask", 0),
                        Tarih = DateTime.UtcNow // UTC olarak değiştirildi
                    };
var uygunsuzluklar = new List<int>();

// Python çıktısındaki Ppe_counts sözlüğünde anahtar var mı diye kontrol ediyoruz
if (parsedOutput.Ppe_counts.ContainsKey("NO-Gloves")) // Düzeltme burada!
    uygunsuzluklar.Add(1);
if (parsedOutput.Ppe_counts.ContainsKey("NO-Goggles")) // Düzeltme burada!
    uygunsuzluklar.Add(2);
if (parsedOutput.Ppe_counts.ContainsKey("NO-Hardhat")) // Düzeltme burada!
    uygunsuzluklar.Add(3);
if (parsedOutput.Ppe_counts.ContainsKey("NO-Mask")) // Düzeltme burada!
    uygunsuzluklar.Add(4);
if (parsedOutput.Ppe_counts.ContainsKey("NO-Safety Vest")) // Düzeltme burada!
    uygunsuzluklar.Add(5);

// Rapor kaydından sonra bu ID'lerle bağlantı kur
foreach (var uygunsuzlukId in uygunsuzluklar)
{
    var ru = new RaporUygunsuzluk
    {
        RaporId = rapor.Id,
        UygunsuzlukId = uygunsuzlukId
    };
    _context.RaporUygunsuzluklar.Add(ru);
}

await _context.SaveChangesAsync();

                    // Raporu veritabanı bağlamına ekler
                    _context.Raporlar.Add(rapor);
                    // Değişiklikleri veritabanına kaydeder
                    await _context.SaveChangesAsync();

                    // Sonuçları göstermek için Result View'ını döndürür
                    return View("~/Views/Rapor/Result.cshtml");
                }
                else
                {
                    // --- Python Betiği Hata Durumu ---
                    // Eğer Python betiği hata koduyla çıkarsa, hata çıktısını loglar ve hata View'ını döndürür
                    _logger.LogError($"Python betiği hata verdi: {pythonError}\nPython Çıktısı:\n{pythonOutput}");
                    return View("Error", new ErrorViewModel { RequestId = pythonError });
                }
            }
        }
    }
} */
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using ISGKkdTakip.Models;
using ISGKkdTakip.Data;
using ISGKkdTakip.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
namespace ISGKkdTakip.Controllers
{
    // Python'dan gelen JSON çıktısını temsil eden yardımcı sınıf
    public class PythonOutput
    {
        public string Output_image_path { get; set; }
        public Dictionary<string, int> Ppe_counts { get; set; }
        public int Total_person_count { get; set; }
    }

    public class ImagePredictionController : Controller
    {
        private readonly ILogger<ImagePredictionController> _logger;
        private readonly ApplicationDbContext _context;

        public ImagePredictionController(ILogger<ImagePredictionController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Mekan bazlı kıyaslama
    public IActionResult Kiyasla(int mekanId)
{
    var raporlar = _context.Raporlar
        .Where(r => r.MekanId == mekanId)
        .Include(r => r.Mekan)  // Mekan'ı da yükle
        .OrderByDescending(r => r.Tarih)
        .Take(2)
        .ToList();

    if (raporlar.Count < 2)
    {
        ViewBag.Mesaj = "Karşılaştırma için en az 2 rapor olmalıdır.";
        return View();
    }

    var bugun = raporlar[0];
    var onceki = raporlar[1];

    // Farkları hesapla (pozitif veya negatif)
   var farklar = new Dictionary<string, int>
{
    { "EkipmanKullanan", bugun.EkipmanKullanan - onceki.EkipmanKullanan },
    
};

    // Örnek olarak ayrı ayrı kask, yelek, gözlük sayısı varsayıyorum, 
    // eğer ayrı alanlar yoksa modeli buna göre genişletmen gerek.

    // Mekanın adı:
    ViewBag.MekanAdi = bugun.Mekan?.Ad ?? "Bilinmeyen Mekan";

    // Farkları JSON olarak View'a gönderelim
    ViewBag.Farklar = farklar;

    // Önceki ve bugünkü sayıları da gönderebilirsin grafik için:
    ViewBag.Onceki = onceki;
    ViewBag.Bugun = bugun;

     return View("~/Views/Rapor/Kiyasla.cshtml");

}

   



        // GET: Görsel yükleme sayfası
        [HttpGet]
        public IActionResult Index()
        {
            var mekanlar = _context.Mekanlar
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
                .ToList();

            var viewModel = new ImageUploadViewModel
            {
                MekanList = mekanlar
            };

            ViewBag.Mekanlar = mekanlar;
            return View(viewModel);
        }

        // POST: Görsel analiz
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(ImageUploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                ViewBag.ErrorMessage = "Lütfen analiz etmek için bir görsel seçin.";

                model.MekanList = _context.Mekanlar
                    .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
                    .ToList();

                return View("Index", model);
            }

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string inputFileName = "uploaded_image.jpg";
            string outputFileName = "annotated_output.jpg";

            string inputImagePath = Path.Combine(uploadsFolder, inputFileName);
            string outputImagePath = Path.Combine(uploadsFolder, outputFileName);

            try
            {
                if (System.IO.File.Exists(inputImagePath))
                    System.IO.File.Delete(inputImagePath);
                if (System.IO.File.Exists(outputImagePath))
                    System.IO.File.Delete(outputImagePath);
            }
            catch (IOException ex)
            {
                _logger.LogError($"Dosya silme hatası: {ex.Message}");
                ViewBag.ErrorMessage = "Geçici dosyalar silinemedi.";
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }

            using (var stream = new FileStream(inputImagePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            var psi = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"draw_predictions.py \"{inputImagePath}\" \"{outputImagePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            string pythonOutput;
            string pythonError;

            using (var process = Process.Start(psi))
            {
                pythonOutput = await process.StandardOutput.ReadToEndAsync();
                pythonError = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    PythonOutput parsedOutput = null;
                    try
                    {
                        var lastJsonLine = pythonOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .LastOrDefault(line => line.Trim().StartsWith("{") && line.Trim().EndsWith("}"));

                        if (!string.IsNullOrEmpty(lastJsonLine))
                        {
                            parsedOutput = JsonSerializer.Deserialize<PythonOutput>(lastJsonLine, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"Python çıktısı JSON ayrıştırma hatası: {ex.Message}\nÇıktı:\n{pythonOutput}");
                        return View("Error", new ErrorViewModel { RequestId = "JSON ayrıştırma hatası." });
                    }

                    if (parsedOutput == null || parsedOutput.Ppe_counts == null)
                    {
                        _logger.LogError($"Python çıktısı boş veya beklenmeyen formatta: {pythonOutput}");
                        return View("Error", new ErrorViewModel { RequestId = "Python çıktısı geçersiz." });
                    }

                    ViewBag.ProcessedImagePath = $"/uploads/{outputFileName}";

                    ViewBag.Hardhats = parsedOutput.Ppe_counts.GetValueOrDefault("hardhat", 0);
                    ViewBag.Vests = parsedOutput.Ppe_counts.GetValueOrDefault("safety vest", 0);
                    ViewBag.Goggles = parsedOutput.Ppe_counts.GetValueOrDefault("goggles", 0);
                    ViewBag.Masks = parsedOutput.Ppe_counts.GetValueOrDefault("mask", 0);
                    ViewBag.TotalPeople = parsedOutput.Total_person_count;

                    ViewBag.MekanId = model.SelectedMekanId;
                    ViewBag.Mekanlar = _context.Mekanlar
                        .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Ad })
                        .ToList();

                    var rapor = new Rapor
                    {
                        MekanId = model.SelectedMekanId,
                        UygunsuzlukId = 1,
                        ToplamKisi = parsedOutput.Total_person_count,
                        EkipmanKullanan = parsedOutput.Ppe_counts.GetValueOrDefault("hardhat", 0) +
                                          parsedOutput.Ppe_counts.GetValueOrDefault("safety vest", 0) +
                                          parsedOutput.Ppe_counts.GetValueOrDefault("goggles", 0) +
                                          parsedOutput.Ppe_counts.GetValueOrDefault("mask", 0),
                        Tarih = DateTime.UtcNow
                    };

                    var uygunsuzluklar = new List<int>();

                    if (parsedOutput.Ppe_counts.ContainsKey("NO-Gloves"))
                        uygunsuzluklar.Add(1);
                    if (parsedOutput.Ppe_counts.ContainsKey("NO-Goggles"))
                        uygunsuzluklar.Add(2);
                    if (parsedOutput.Ppe_counts.ContainsKey("NO-Hardhat"))
                        uygunsuzluklar.Add(3);
                    if (parsedOutput.Ppe_counts.ContainsKey("NO-Mask"))
                        uygunsuzluklar.Add(4);
                    if (parsedOutput.Ppe_counts.ContainsKey("NO-Safety Vest"))
                        uygunsuzluklar.Add(5);

                    _context.Raporlar.Add(rapor);
                    await _context.SaveChangesAsync();

                    foreach (var uygunsuzlukId in uygunsuzluklar)
                    {
                        var ru = new RaporUygunsuzluk
                        {
                            RaporId = rapor.Id,
                            UygunsuzlukId = uygunsuzlukId
                        };
                        _context.RaporUygunsuzluklar.Add(ru);
                    }

                    await _context.SaveChangesAsync();

                    var resultViewModel = new ImageUploadViewModel
                    {
                        SelectedMekanId = model.SelectedMekanId,
                        MekanList = ViewBag.Mekanlar
                    };

                    return View("~/Views/Rapor/Result.cshtml", resultViewModel);
                }
                else
                {
                    _logger.LogError($"Python betiği hata verdi: {pythonError}\nPython Çıktısı:\n{pythonOutput}");
                    return View("Error", new ErrorViewModel { RequestId = pythonError });
                }
            }
        }
    }
}
