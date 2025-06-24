namespace FoodTour.API.Models
{
    public class PostImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PostId { get; set; }
        public string ImageUrl { get; set; }

        public Post Post { get; set; }
    }
}
