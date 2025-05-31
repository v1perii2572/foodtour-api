using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FoodTour.API.Services
{
    public class GoogleGeocodingService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public GoogleGeocodingService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        // Lấy tọa độ (lat, lng) từ địa chỉ
        public async Task<(double lat, double lng)> GetLatLngAsync(string address)
        {
            var apiKey = _config["Google:GoogleApiKey"];
            var fullUrl = $"https://maps.gomaps.pro/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

            try
            {
                var response = await _http.GetAsync(fullUrl);
                var jsonString = await response.Content.ReadAsStringAsync();

                using var json = JsonDocument.Parse(jsonString);

                if (json.RootElement.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var loc = results[0].GetProperty("geometry").GetProperty("location");
                    return (loc.GetProperty("lat").GetDouble(), loc.GetProperty("lng").GetDouble());
                }

                return (0, 0);
            }
            catch
            {
                return (0, 0);
            }
        }

        // Lấy địa chỉ từ tọa độ (lat, lng)
        public async Task<string> ReverseGeocodeAsync(double lat, double lng)
        {
            var apiKey = _config["Google:GoogleApiKey"];
            var url = $"https://maps.gomaps.pro/maps/api/geocode/json?latlng={lat},{lng}&key={apiKey}";

            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return "Hà Nội";
                }

                var content = await response.Content.ReadAsStringAsync();

                using var jsonDoc = JsonDocument.Parse(content);

                var results = jsonDoc.RootElement.GetProperty("results");
                if (results.GetArrayLength() == 0)
                {
                    return "Hà Nội";
                }

                var address = results[0].GetProperty("formatted_address").GetString();
                return address ?? "Hà Nội";
            }
            catch
            {
                return "Hà Nội";
            }
        }
    }
}
