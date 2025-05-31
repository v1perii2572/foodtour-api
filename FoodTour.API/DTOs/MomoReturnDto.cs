namespace FoodTour.API.DTOs
{
    public class MomoReturnDto
    {
        public string PartnerCode { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public string Amount { get; set; }
        public string Message { get; set; }
        public string ResultCode { get; set; }
        public string Signature { get; set; }
        public string ResponseTime { get; set; }
        public string ExtraData { get; set; }
    }
}
