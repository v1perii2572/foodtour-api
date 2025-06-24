namespace FoodTour.API.DTOs
{
    public class CreatePostDto
    {
        public string Content { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public Guid UserId { get; set; }
    }
}
