namespace FoodTour.API.DTOs
{
    public class ConfirmRouteDto
    {
        public Guid SessionId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<RoutePlaceDto> Places { get; set; } = new();
    }
}
