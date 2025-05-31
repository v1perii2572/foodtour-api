namespace FoodTour.API.DTOs
{
    public class DefaultRouteDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<PlaceDto> Places { get; set; } = new();
    }

    public class PlaceDto
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string? Role { get; set; }
        public string? Note { get; set; }
        public string? TimeSlot { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
    }
}
