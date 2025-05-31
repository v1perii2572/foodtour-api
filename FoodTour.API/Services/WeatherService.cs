using System.Text.Json;

namespace FoodTour.API.Services
{
    public class WeatherService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public WeatherService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<(string Description, double Temp)> GetWeatherAsync(double lat, double lng)
        {
            var apiKey = _config["OpenWeather:ApiKey"];
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lng}&units=metric&appid={apiKey}";

            var response = await _http.GetFromJsonAsync<JsonElement>(url);

            var weatherDesc = response.GetProperty("weather")[0].GetProperty("description").GetString() ?? "Không rõ";
            var temp = response.GetProperty("main").GetProperty("temp").GetDouble();

            return (weatherDesc, temp);
        }
    }
}
