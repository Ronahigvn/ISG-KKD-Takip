using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ISGKkdTakip.Controllers
{
    public class RaporController : Controller
    {
        private readonly HttpClient _httpClient;

        public RaporController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("api/uploadimage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya yüklenmedi.");

            var fastApiUrl = "http://127.0.0.1:8000/tahmin";

            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync(fastApiUrl, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "FastAPI servisi hata verdi.");

            var resultJson = await response.Content.ReadAsStringAsync();

            return Content(resultJson, "application/json");
        }
    }
}
