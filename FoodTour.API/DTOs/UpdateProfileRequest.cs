namespace FoodTour.API.DTOs
{
    public class UpdateProfileRequest
    {
        public string Name { get; set; }
        public List<string> DislikedFoods { get; set; }
    }
}
