namespace FoodTour.API.DTOs
{
    public class RoutePlaceDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
