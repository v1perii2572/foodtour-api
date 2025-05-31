using FoodTour.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace FoodTour.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PlacesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("nearby-google")]
        public async Task<IActionResult> GetNearbyFromGoogle([FromQuery] double lat, [FromQuery] double lng, [FromQuery] int radius = 2000)
        {
            var apiKey = _configuration["Google:GoogleApiKey"];
            var url = $"https://maps.gomaps.pro/maps/api/place/nearbysearch/json?location={lat},{lng}&radius={radius}&type=restaurant&key={apiKey}";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Lỗi khi gọi Google Places API");
            }

            var result = await response.Content.ReadAsStringAsync();
            return Content(result, "application/json");
        }
    }
}
