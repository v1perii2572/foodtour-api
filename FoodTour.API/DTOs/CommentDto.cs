namespace FoodTour.API.DTOs
{
    public class CommentDto
    {
        public string UserEmail { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
