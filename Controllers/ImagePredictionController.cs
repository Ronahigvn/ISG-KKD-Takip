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
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);                         // yoksa oluştur
            var savedPath   = Path.Combine(uploadsPath, "uploaded_image.jpg");
            await using (var fs = System.IO.File.Create(savedPath))
            {
                await file.CopyToAsync(fs);
            }

            /* -----------------------------------------------------------
             * 2) FastAPI (Python) servisine resmi gönder, JSON sonucu al
             * ----------------------------------------------------------*/
            var form = new MultipartFormDataContent
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

            /* -----------------------------------------------------------
             * 3) Bounding‑box’lı görseli üretmek için Python betiğini çağır
             *    (draw_predictions.py input output parametreleriyle)
             * ----------------------------------------------------------*/
            var annotatedPath = Path.Combine(uploadsPath, "annotated_output.jpg");

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

            /* -----------------------------------------------------------
             * 4) Sonuçları ViewBag ile görünüme aktar
             * ----------------------------------------------------------*/
            ViewBag.TotalPeople = (int)(json["total_predictions"] ?? 0);
            ViewBag.Hardhats    = (int)(json["hardhats"] ?? 0);
            ViewBag.Vests       = (int)(json["vests"] ?? 0);
            ViewBag.Goggles     = (int)(json["goggles"] ?? 0);
            ViewBag.Masks       = (int)(json["masks"] ?? 0);
            ViewBag.AnnotatedImg = "/uploads/annotated_output.jpg"; // <img src='...' />

            return View("~/Views/Rapor/Result.cshtml");
        }
    }
}


