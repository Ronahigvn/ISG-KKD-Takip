/* using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

public class ImagePredictionController : Controller
{
    private readonly HttpClient _httpClient;

    public ImagePredictionController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    // KKD’yi belirleyen ufak yardımcı:
    private static bool IsKkd(JToken pred) =>
        pred?["class"]?.ToString().ToLowerInvariant() switch
        {
            "kask"       => true,   // kask
            "helmet"     => true,   // İngilizce kask
            "mask"       => true,   // varsa maske
            "vest"       => true,   // yelek
            _            => false
        };

    [HttpPost]
    public async Task<IActionResult> AnalyzeImage(IFormFile file)
    {
        if (file is null or { Length: 0 })
        {
            ViewBag.Error = "Lütfen bir resim yükleyin.";
            return View("~/Views/Home/Index.cshtml");
        }

        // resmi multipart/form-data ile aktar
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);

        try
        {
            var apiUrl = "http://127.0.0.1:8000/analyze/";
            var response = await _httpClient.PostAsync(apiUrl, form);
            response.EnsureSuccessStatusCode();

            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
            var preds = json["predictions"] ?? new JArray();

            int toplamKisi   = preds.Count();
            int kkdKullanan  = preds.Count(IsKkd);
            int kkdKullanmayan = toplamKisi - kkdKullanan;

            // DEBUG log
            Console.WriteLine($"""
               === AnalyzeImage ===
               Toplam kişi      : {toplamKisi}
               KKD kullanan     : {kkdKullanan}
               KKD kullanmayan  : {kkdKullanmayan}
               ====================
               """);

            // ViewBag ile görünüme geçir
            ViewBag.TotalPeople = toplamKisi;
            ViewBag.WithKkd     = kkdKullanan;
            ViewBag.WithoutKkd  = kkdKullanmayan;

            return View("~/Views/Rapor/Result.cshtml");
        }
        catch (Exception ex)
        {
            Console.WriteLine("API hatası → " + ex.Message);
            ViewBag.Error = "API isteği sırasında hata: " + ex.Message;
            return View("~/Views/Rapor/Index.cshtml");
        }
    }
}
 */
/* 
 using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

public class ImagePredictionController : Controller
{
    private readonly HttpClient _httpClient;

    public ImagePredictionController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "Lütfen bir resim yükleyin.";
            return View("~/Views/Home/Index.cshtml");
        }

        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        try
        {
            var response = await _httpClient.PostAsync("http://127.0.0.1:8000/analyze/", content);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonString);

            // API'den gelen verileri oku
            int toplamKisi     = (int)(json["total_predictions"] ?? 0);
            int kaskSayisi     = (int)(json["hardhats"] ?? 0);
            int yelekSayisi    = (int)(json["vests"] ?? 0);
            int gozlukSayisi   = (int)(json["goggles"] ?? 0);
          

            Console.WriteLine($"""
                === AnalyzeImage (Yeni Model) ===
                Toplam tespit       : {toplamKisi}
                Kask                : {kaskSayisi}
                Yelek               : {yelekSayisi}
                Gözlük              : {gozlukSayisi}
             
                ================================
            """);

            // ViewBag ile görünüme aktar
            ViewBag.TotalPeople = toplamKisi;
            ViewBag.Hardhats    = kaskSayisi;
            ViewBag.Vests       = yelekSayisi;
            ViewBag.Goggles     = gozlukSayisi;
          

            return View("~/Views/Rapor/Result.cshtml");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("API isteği sırasında hata oluştu: " + ex.Message);
            ViewBag.Error = "API isteği sırasında hata: " + ex.Message;
            return View("~/Views/Rapor/Index.cshtml");
        }
    }
} */
/* 
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

public class ImagePredictionController : Controller
{
    private readonly HttpClient _httpClient;

    public ImagePredictionController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "Lütfen bir resim yükleyin.";
            return View("~/Views/Home/Index.cshtml");
        }

        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);

        try
        {
            var apiUrl = "http://127.0.0.1:8000/analyze/";
            var response = await _httpClient.PostAsync(apiUrl, form);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseString);

            // JSON veriler
            int toplamTespit   = (int)(json["total_predictions"] ?? 0);
            int kask           = (int)(json["hardhats"] ?? 0);
            int yelek          = (int)(json["vests"] ?? 0);
            int gozluk         = (int)(json["goggles"] ?? 0);
            int maske          = (int)(json["masks"] ?? 0);

            Console.WriteLine($"""
                === AnalyzeImage ===
                Toplam Tespit   : {toplamTespit}
                Kask            : {kask}
                Yelek           : {yelek}
                Gözlük          : {gozluk}
                Maske           : {maske}
                =====================
            """);

            ViewBag.TotalPeople = toplamTespit;
            ViewBag.Hardhats    = kask;
            ViewBag.Vests       = yelek;
            ViewBag.Goggles     = gozluk;
            ViewBag.Masks       = maske;

            return View("~/Views/Rapor/Result.cshtml");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("API Hatası: " + ex.Message);
            ViewBag.Error = "API isteği sırasında hata: " + ex.Message;
            return View("~/Views/Rapor/Index.cshtml");
        }
    }
}

 */

/*  using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ISGKkdTakip.Controllers
{
    public class ImagePredictionController : Controller
    {
        private readonly HttpClient _httpClient;

        // ctor’da IHttpClientFactory kullan – singleton sorunlarını önler
        public ImagePredictionController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        [HttpPost]
        [Route("kkd/analyze")]   // ⇒  POST /kkd/analyze   (form action’ında bunu kullan)
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Lütfen bir resim yükleyin.";
                return RedirectToAction("Index", "Home");
            }

            // multipart form-data hazırla
            await using var stream = file.OpenReadStream();
            var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(stream)
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
                    },
                    "file",
                    file.FileName
                }
            };

            try
            {
                const string apiUrl = "http://127.0.0.1:8000/analyze/";
                var resp = await _httpClient.PostAsync(apiUrl, content);
                resp.EnsureSuccessStatusCode();

                // ------ JSON verisini oku ------
                var json   = JObject.Parse(await resp.Content.ReadAsStringAsync());
                int total  = (int)(json["total_predictions"] ?? 0);
                int hats   = (int)(json["hardhats"]          ?? 0);
                int vests  = (int)(json["vests"]             ?? 0);
                int goggs  = (int)(json["goggles"]           ?? 0);
                int masks  = (int)(json["masks"]             ?? 0);

                // ViewBag yerine ViewData Model suğunu kullanabilirsin ama şimdilik ViewBag yeter
                ViewBag.TotalPeople = total;
                ViewBag.Hardhats    = hats;
                ViewBag.Vests       = vests;
                ViewBag.Goggles     = goggs;
                ViewBag.Masks       = masks;

                return View("~/Views/Rapor/Result.cshtml");
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API hatası: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
 */

/* 
 using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;

namespace ISGKkdTakip.Controllers
{
    public class ImagePredictionController : Controller
    {
        private readonly HttpClient _httpClient;

        public ImagePredictionController(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        [HttpPost]
        [Route("kkd/analyze")]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Lütfen bir resim yükleyin.";
                return RedirectToAction("Index", "Home");
            }

            // === 1. Adım: Yüklenen resmi wwwroot/uploads klasörüne kaydet ===
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath); // yoksa oluştur
            var savedImagePath = Path.Combine(uploadsPath, "uploaded_image.jpg");

            await using (var stream = new FileStream(savedImagePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // === 2. Adım: FastAPI sunucusuna resmi gönder ===
            using var form = new MultipartFormDataContent();
            await using var apiStream = System.IO.File.OpenRead(savedImagePath);
            var fileContent = new StreamContent(apiStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "file", "uploaded_image.jpg");

            try
            {
                const string apiUrl = "http://127.0.0.1:8000/analyze/";
                var resp = await _httpClient.PostAsync(apiUrl, form);
                resp.EnsureSuccessStatusCode();

                var json = JObject.Parse(await resp.Content.ReadAsStringAsync());
                int total  = (int)(json["total_predictions"] ?? 0);
                int hats   = (int)(json["hardhats"]          ?? 0);
                int vests  = (int)(json["vests"]             ?? 0);
                int goggs  = (int)(json["goggles"]           ?? 0);
                int masks  = (int)(json["masks"]             ?? 0);

                ViewBag.TotalPeople = total;
                ViewBag.Hardhats    = hats;
                ViewBag.Vests       = vests;
                ViewBag.Goggles     = goggs;
                ViewBag.Masks       = masks;

                // === 3. Adım: draw_predictions.py dosyasını çalıştır ===
                var psi = new ProcessStartInfo
                {
                    FileName = "python3", // veya sadece "python"
                    Arguments = "draw_predictions.py",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine("❌ draw_predictions.py HATA: " + error);
                }

                return View("~/Views/Rapor/Result.cshtml");
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API hatası: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
 */
/* using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ISGKkdTakip.Controllers
{
    public class ImagePredictionController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _env;

        public ImagePredictionController(
            IHttpClientFactory factory,
            IWebHostEnvironment env)
        {
            _httpClient = factory.CreateClient();
            _env        = env;
        }

        /// <summary>
        /// KKD analizi yapar, grafiği ve işlenmiş resmi sonuç sayfasında gösterir.
        /// </summary>
        [HttpPost]
        [Route("kkd/analyze")]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Error"] = "Lütfen bir resim yükleyin.";
                return RedirectToAction("Index", "Home");
            }

            /* -----------------------------------------------------------
             * 1) Yüklenen dosyayı wwwroot/uploads klasörüne kaydet
             * ----------------------------------------------------------*/
         /*    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);                         // yoksa oluştur
            var savedPath   = Path.Combine(uploadsPath, "uploaded_image.jpg");
            await using (var fs = System.IO.File.Create(savedPath))
            {
                await file.CopyToAsync(fs);
            }
 */
            /* -----------------------------------------------------------
             * 2) FastAPI (Python) servisine resmi gönder, JSON sonucu al
             * ----------------------------------------------------------*/
          /*   var form = new MultipartFormDataContent
            {
                {
                    new StreamContent(System.IO.File.OpenRead(savedPath))
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
                    },
                    "file",
                    "uploaded_image.jpg"
                }
            };
 */
          /*   HttpResponseMessage resp;
            try
            {
                resp = await _httpClient.PostAsync("http://127.0.0.1:8000/analyze/", form);
                resp.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API hatası: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }

            var json = JObject.Parse(await resp.Content.ReadAsStringAsync());
 */
            /* -----------------------------------------------------------
             * 3) Bounding‑box’lı görseli üretmek için Python betiğini çağır
             *    (draw_predictions.py input output parametreleriyle)
             * ----------------------------------------------------------*/
           /*  var annotatedPath = Path.Combine(uploadsPath, "annotated_output.jpg");

            var psi = new ProcessStartInfo("python")
            {
                ArgumentList =
                {
                    "draw_predictions.py",
                    savedPath,          // input
                    annotatedPath       // output
                },
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(psi);
            proc!.WaitForExit();
            if (proc.ExitCode != 0)
            {
                TempData["Error"] = "Python betiği hata verdi:\n" + await proc.StandardError.ReadToEndAsync();
                return RedirectToAction("Index", "Home");
            }
 */
            /* -----------------------------------------------------------
             * 4) Sonuçları ViewBag ile görünüme aktar
             * ----------------------------------------------------------*/
         /*    ViewBag.TotalPeople = (int)(json["total_predictions"] ?? 0);
            ViewBag.Hardhats    = (int)(json["hardhats"] ?? 0);
            ViewBag.Vests       = (int)(json["vests"] ?? 0);
            ViewBag.Goggles     = (int)(json["goggles"] ?? 0);
            ViewBag.Masks       = (int)(json["masks"] ?? 0);
            ViewBag.AnnotatedImg = "/uploads/annotated_output.jpg"; // <img src='...' />

            return View("~/Views/Rapor/Result.cshtml");
        }
    }
}
 */
/* 
 using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ISGKkdTakip.Controllers
{
    public class ImagePredictionController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _env;

        public ImagePredictionController(
            IHttpClientFactory factory,
            IWebHostEnvironment env)
        {
            _httpClient = factory.CreateClient();
            _env        = env;
        }

        [HttpPost]
        [Route("kkd/analyze")]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Error"] = "Lütfen bir resim yükleyin.";
                return RedirectToAction("Index", "Home");
            }

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            var uniqueName = $"uploaded_{Guid.NewGuid():N}.jpg";
            var savedPath = Path.Combine(uploadsPath, uniqueName);

            await using (var fs = System.IO.File.Create(savedPath))
            {
                await file.CopyToAsync(fs);
            }

            var form = new MultipartFormDataContent
            {
                {
                    new StreamContent(System.IO.File.OpenRead(savedPath))
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
                    },
                    "file",
                    uniqueName
                }
            };

            HttpResponseMessage resp;
            try
            {
                resp = await _httpClient.PostAsync("http://127.0.0.1:8000/analyze/", form);
                resp.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"API hatası: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }

            var json = JObject.Parse(await resp.Content.ReadAsStringAsync());

            var annotatedName = $"annotated_{Guid.NewGuid():N}.jpg";
            var annotatedPath = Path.Combine(uploadsPath, annotatedName);

            var psi = new ProcessStartInfo("python")
            {
                ArgumentList =
                {
                    "draw_predictions.py",
                    savedPath,
                    annotatedPath
                },
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(psi);
            proc!.WaitForExit();
            if (proc.ExitCode != 0)
            {
                TempData["Error"] = "Python betiği hata verdi:\n" + await proc.StandardError.ReadToEndAsync();
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TotalPeople  = (int)(json["total_predictions"] ?? 0);
            ViewBag.Hardhats     = (int)(json["hardhats"] ?? 0);
            ViewBag.Vests        = (int)(json["vests"] ?? 0);
            ViewBag.Goggles      = (int)(json["goggles"] ?? 0);
            ViewBag.Masks        = (int)(json["masks"] ?? 0);
            ViewBag.AnnotatedImg = $"/uploads/{annotatedName}";
            return View("~/Views/Rapor/Result.cshtml");


        }
    }
} */
using System.Collections.Generic; // For Dictionary
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // For logging
using System.IO; // For file operations
using System.Diagnostics; // For running external processes (Python script)
using System.Threading.Tasks; // For async/await operations
using ISGKkdTakip.Models; // Assuming ErrorViewModel is in this namespace

namespace ISGKkdTakip.Controllers
{
    public class ImagePredictionController : Controller
    {
        private readonly ILogger<ImagePredictionController> _logger;
        // Eğer wwwroot yolunu almak için IWebHostEnvironment kullanıyorsanız, yorum satırını kaldırın ve constructor'a ekleyin.
        // private readonly IWebHostEnvironment _env; 

        public ImagePredictionController(ILogger<ImagePredictionController> logger /*, IWebHostEnvironment env */)
        {
            _logger = logger;
            // _env = env;
        }

        // Bu metod, HTTP POST isteğiyle bir görsel dosyası alır ve işler.
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            // Eğer dosya boş veya seçilmemişse hata mesajı göster.
            if (file == null || file.Length == 0)
            {
                ViewBag.ErrorMessage = "Lütfen analiz etmek için bir görsel seçin.";
                // Hata mesajını göstermek için Result.cshtml'ye yönlendirebilirsiniz
                return View("~/Views/Rapor/Result.cshtml"); 
            }

            // --- 1. Yüklenen Görseli Kaydetme ---
            // Görsellerin kaydedileceği wwwroot/uploads klasörünün yolunu belirle.
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            
            // Eğer klasör yoksa oluştur.
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Python betiğinin beklediği girdi ve çıktı dosya isimlerini tanımla.
            string inputFileName = "uploaded_image.jpg"; // Python betiği bu isimle okuyacak
            string outputFileName = "annotated_output.jpg"; // Python betiği bu isimle kaydedecek

            string inputImagePath = Path.Combine(uploadsFolder, inputFileName);
            string outputImagePath = Path.Combine(uploadsFolder, outputFileName);

            // Önceki yüklemelerden kalma aynı isimdeki dosyayı silmeye çalış.
            // Bu, dosya kilitlenme hatalarını önlemeye yardımcı olabilir.
            try
            {
                if (System.IO.File.Exists(inputImagePath))
                {
                    System.IO.File.Delete(inputImagePath);
                }
                if (System.IO.File.Exists(outputImagePath)) // İşlenmiş çıktı dosyasını da temizle
                {
                    System.IO.File.Delete(outputImagePath);
                }
            }
            catch (IOException ex)
            {
                _logger.LogError($"Eski görsel dosyaları silinemedi: {ex.Message}");
                ViewBag.ErrorMessage = "Sunucu geçici dosyaları temizleyemedi. Lütfen tekrar deneyin.";
                return View("Error", new ErrorViewModel { RequestId = "Dosya temizleme hatası: " + ex.Message });
            }

            // Yüklenen görseli belirtilen yola kaydet. 'using' ifadesi dosyanın düzgün kapatılmasını sağlar.
            using (var stream = new FileStream(inputImagePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            _logger.LogInformation($"Görsel kaydedildi: {inputImagePath}");


            // --- 2. Python Betiğini Çalıştırma ---
            var psi = new ProcessStartInfo();
            // Python yorumlayıcısının yolunu belirt. Linux'ta genellikle 'python3' veya 'python'dur.
            // Windows'ta 'python.exe' veya sadece 'python' olabilir (PATH'e ekliyse).
            psi.FileName = "python3"; 
            
            // Python betiğine giriş ve çıkış dosya yollarını argüman olarak gönder.
            // Dosya yollarında boşluk olabileceği için tırnak içine almak önemlidir.
            psi.Arguments = $"draw_predictions.py \"{inputImagePath}\" \"{outputImagePath}\"";
            
            psi.UseShellExecute = false;       // Kabuk kullanmadan doğrudan işlemi başlat.
            psi.RedirectStandardOutput = true; // Python betiğinin standart çıktı akışını yakala.
            psi.RedirectStandardError = true;  // Python betiğinin hata çıktı akışını yakala.
            psi.CreateNoWindow = true;         // Yeni bir konsol penceresi açma.

            _logger.LogInformation($"Python betiği çağrılıyor: {psi.FileName} {psi.Arguments}");

            string pythonOutput = string.Empty;
            string pythonError = string.Empty;

            // Python betiğini başlat ve çıktılarını oku.
            using (var process = Process.Start(psi))
            {
                // Python betiği tamamlanana kadar bekle ve çıktıları asenkron olarak oku.
                pythonOutput = await process.StandardOutput.ReadToEndAsync();
                pythonError = await process.StandardError.ReadToEndAsync();
                process.WaitForExit(); // Betiğin tamamen bitmesini bekle.

                _logger.LogInformation($"Python Standard Çıktısı:\n{pythonOutput}");
                _logger.LogError($"Python Hata Çıktısı:\n{pythonError}");

                // --- 3. Python Betiği Sonucunu İşleme ---
                // Python betiği başarıyla çalıştıysa (çıkış kodu 0).
                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Python betiği başarıyla çalıştı.");
                    
                    // İşlenmiş görselin web'den erişilebilir yolunu ViewBag'e ekle.
                    // wwwroot klasörü web sunucusu tarafından doğrudan sunulur, bu yüzden /uploads/path kullanılır.
                    ViewBag.ProcessedImagePath = $"/uploads/{outputFileName}"; 

                    // KKE sayımlarını Python çıktısından alıp ViewBag'e ekle.
                    // Python betiğinizin çıktısının "Hardhat: 2\nSafety Vest: 1\n..." formatında olduğu varsayılır.
                    try
                    {
                        var ppeCounts = new Dictionary<string, int>
                        {
                            {"hardhat", 0}, 
                            {"safety vest", 0}, 
                            {"goggles", 0}, 
                            {"mask", 0}
                        };

                        // Python çıktısını satırlara ayır ve boş satırları kaldır.
                        var lines = pythonOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        
                        // Her bir çıktı satırını işle.
                        foreach (var line in lines)
                        {
                            // Eğer satır ':' içeriyorsa (yani bir anahtar-değer çifti ise).
                            if (line.Contains(":"))
                            {
                                var parts = line.Split(':');
                                if (parts.Length == 2)
                                {
                                    // Sınıf adını temizle, küçük harfe çevir ve boşlukları kaldır.
                                    string ppeType = parts[0].Trim().ToLower().Replace(" ", ""); 
                                    
                                    // 'safetyvest' gibi özel durumları 'safety vest' olarak düzelt.
                                    if (ppeType == "safetyvest") ppeType = "safety vest"; 
                                    
                                    // Sayı değerini ayrıştırmaya çalış.
                                    if (int.TryParse(parts[1].Trim(), out int count))
                                    {
                                        // Eğer ayrıştırılan KKE türü sözlükte varsa, sayısını güncelle.
                                        if (ppeCounts.ContainsKey(ppeType))
                                        {
                                            ppeCounts[ppeType] = count;
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Ayrıştırılan sayımları ViewBag'e ata.
                        ViewBag.Hardhats = ppeCounts["hardhat"];
                        ViewBag.Vests = ppeCounts["safety vest"]; 
                        ViewBag.Goggles = ppeCounts["goggles"];
                        ViewBag.Masks = ppeCounts["mask"];
                        
                        // Toplam insan sayısını doğrudan KKE sayılarından tahmin etmek zor olabilir.
                        // Eğer modeliniz 'person' gibi bir sınıfı da algılıyorsa, onu da sayıp atayabilirsiniz.
                        // Şimdilik "Veri Yok" olarak bırakıldı.
                        ViewBag.TotalPeople = "Veri Yok"; 

                        _logger.LogInformation($"KKE Sayımları: Hardhats={ViewBag.Hardhats}, Vests={ViewBag.Vests}, Goggles={ViewBag.Goggles}, Masks={ViewBag.Masks}");

                    }
                    catch (Exception ex) // Python çıktısı ayrıştırılırken bir hata oluşursa
                    {
                        _logger.LogError($"Python çıktısı parse edilirken hata oluştu: {ex.Message}");
                        // Hata durumunda ViewBag değerlerini sıfırla veya varsayılan yap.
                        ViewBag.Hardhats = 0;
                        ViewBag.Vests = 0;
                        ViewBag.Goggles = 0;
                        ViewBag.Masks = 0;
                        ViewBag.TotalPeople = "Veri Çekilemedi";
                    }

                    // Başarılı işlem sonrası sonuçları göstermek için Rapor/Result View'ına yönlendir.
                    return View("~/Views/Rapor/Result.cshtml");
                }
                else // Python betiği hata kodu ile çıktıysa
                {
                    _logger.LogError($"Python betiği beklenenden farklı bir kodla çıktı: {process.ExitCode}. Hata: {pythonError}");
                    // Kullanıcıya hata mesajı göstermek için Error View'ına yönlendir.
                    return View("Error", new ErrorViewModel { RequestId = "Görsel işleme hatası (Python): " + pythonError });
                }
            }
        }
    }
}