namespace FoodTour.API.Models
{
    public class Post
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public ICollection<PostImage> Images { get; set; }
        public ICollection<PostComment> Comments { get; set; }
        public ICollection<PostLike> Likes { get; set; }
    }

}
