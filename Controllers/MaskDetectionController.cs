using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ISGKkdTakip.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaskDetectionController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MaskDetectionController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya yüklenmedi.");

            var client = _httpClientFactory.CreateClient();
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            content.Add(new StreamContent(fileStream), "file", file.FileName);

            var roboflowApiKey = "1nP8NxJMP9QsCHjudTOy";
            var modelUrl = "https://infer.roboflow.com/ppe-8k2vo/3";

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", roboflowApiKey);

            var response = await client.PostAsync(modelUrl, content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Roboflow API hatası");

            var result = await response.Content.ReadAsStringAsync();

            return Ok(result);
        }
    }
}

