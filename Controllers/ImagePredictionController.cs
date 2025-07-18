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
