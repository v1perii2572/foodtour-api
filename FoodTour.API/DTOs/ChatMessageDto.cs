namespace FoodTour.API.DTOs
{
    public class ChatMessageDto
    {
        public Guid? SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

}
