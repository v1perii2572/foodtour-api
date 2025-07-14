using System.Net.Http.Headers;
using System.Text.Json;

namespace FoodTour.API.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public GeminiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> GetSuggestionAsync(string userPrompt)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = userPrompt }
                }
            }
        }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API lỗi: {(int)response.StatusCode} - {response.ReasonPhrase}\n{responseText}");
            }

            try
            {
                var json = JsonDocument.Parse(responseText);

                return json.RootElement
                           .GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0]
                           .GetProperty("text")
                           .GetString()!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Gemini API trả về JSON không hợp lệ:\n{responseText}", ex);
            }
        }
    }
}
