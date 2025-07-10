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

