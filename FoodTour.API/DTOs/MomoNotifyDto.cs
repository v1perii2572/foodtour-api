namespace FoodTour.API.DTOs
{
    public class MomoNotifyDto
    {
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public string Signature { get; set; }

        public string Amount { get; set; }
    }
}
