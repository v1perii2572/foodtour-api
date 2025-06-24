using FoodTour.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodTour.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly CloudinaryService _cloudinary;

        public UploadController(CloudinaryService cloudinary)
        {
            _cloudinary = cloudinary;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected");

            var imageUrl = await _cloudinary.UploadImageAsync(file);
            if (imageUrl == null) return StatusCode(500, "Upload failed");

            return Ok(new { imageUrl });
        }
    }
}
