using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class PlaceController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _googleApiKey = "AIzaSyCXs8s_xJixgYASMvv3zM4sBRommzip7jM";

    public PlaceController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpPost("details")]
    public async Task<IActionResult> GetPlaceDetails([FromBody] PlaceRequest request)
    {
        if (string.IsNullOrEmpty(request.Input))
            return BadRequest("Input is required");

        var findPlaceUrl = $"https://maps.googleapis.com/maps/api/place/findplacefromtext/json?input={System.Net.WebUtility.UrlEncode(request.Input)}&inputtype=textquery&fields=place_id&key={_googleApiKey}";

        var findPlaceResponse = await _httpClient.GetFromJsonAsync<FindPlaceResponse>(findPlaceUrl);

        if (findPlaceResponse?.Candidates == null || findPlaceResponse.Candidates.Count == 0)
        {
            return NotFound("Place not found");
        }

        var placeId = findPlaceResponse.Candidates[0].PlaceId;

        var detailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=name,rating,reviews,photos,formatted_address,formatted_phone_number,opening_hours,website,price_level,user_ratings_total,business_status,url&key={_googleApiKey}";

        var detailsResponse = await _httpClient.GetFromJsonAsync<PlaceDetailsResponse>(detailsUrl);

        if (detailsResponse?.Result == null)
        {
            return NotFound("Không tìm thấy dữ liệu");
        }

        // Tạo URL proxy ảnh với hostname + port backend
        var backendHost = $"{Request.Scheme}://{Request.Host}";

        if (detailsResponse.Result.Photos != null)
        {
            foreach (var photo in detailsResponse.Result.Photos)
            {
                photo.PhotoUrl = $"{backendHost}/api/Place/photo?photoReference={photo.PhotoReference}";
            }
        }

        return Ok(detailsResponse.Result);
    }

    // API proxy ảnh
    [HttpGet("photo")]
    public async Task<IActionResult> GetPhoto([FromQuery] string photoReference, [FromQuery] int maxWidth = 400)
    {
        if (string.IsNullOrEmpty(photoReference))
            return BadRequest("photoReference is required");

        var url = $"https://maps.googleapis.com/maps/api/place/photo?maxwidth={maxWidth}&photoreference={photoReference}&key={_googleApiKey}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
        var stream = await response.Content.ReadAsStreamAsync();

        return File(stream, contentType);
    }
}

public class PlaceRequest
{
    public string Input { get; set; }
}

public class PlaceDetailsResponse
{
    [JsonPropertyName("result")]
    public PlaceResult Result { get; set; }
}

public class PlaceResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("formatted_address")]
    public string FormattedAddress { get; set; }

    [JsonPropertyName("formatted_phone_number")]
    public string FormattedPhoneNumber { get; set; }

    [JsonPropertyName("rating")]
    public float? Rating { get; set; }

    [JsonPropertyName("user_ratings_total")]
    public int? UserRatingsTotal { get; set; }

    [JsonPropertyName("price_level")]
    public int? PriceLevel { get; set; }

    [JsonPropertyName("website")]
    public string Website { get; set; }

    [JsonPropertyName("opening_hours")]
    public OpeningHours OpeningHours { get; set; }

    [JsonPropertyName("reviews")]
    public List<Review> Reviews { get; set; }

    [JsonPropertyName("photos")]
    public List<Photo> Photos { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class OpeningHours
{
    [JsonPropertyName("weekday_text")]
    public List<string> WeekdayText { get; set; }
}

public class Review
{
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class Photo
{
    [JsonPropertyName("photo_reference")]
    public string PhotoReference { get; set; }

    public string PhotoUrl { get; set; }
}

public class FindPlaceResponse
{
    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; set; }
}

public class Candidate
{
    [JsonPropertyName("place_id")]
    public string PlaceId { get; set; }
}
