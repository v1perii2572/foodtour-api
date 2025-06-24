namespace FoodTour.API.DTOs
{
    public class CreateCommentDto
    {
        public Guid UserId { get; set; }
        public string Content { get; set; }
    }
}
