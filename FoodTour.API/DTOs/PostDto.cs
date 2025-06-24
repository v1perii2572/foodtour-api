using FoodTour.API.DTOs;

public class PostDto
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public string UserName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> ImageUrls { get; set; }
    public int LikeCount { get; set; }
    public List<CommentDto> Comments { get; set; }
}
