using System.Net.Http.Headers;
using System.Text.Json;

namespace FoodTour.API.Services
{
    public class CohereAIService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public CohereAIService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> GetSuggestionAsync(string userPrompt)
        {
            var apiKey = _config["Cohere:ApiKey"];
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var request = new
            {
                model = "command-r-plus",
                prompt = userPrompt,
                max_tokens = 300,
                temperature = 0.8
            };

            var response = await _http.PostAsJsonAsync("https://api.cohere.ai/v1/generate", request);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            return json.GetProperty("generations")[0].GetProperty("text").GetString()!;
        }
    }
}
