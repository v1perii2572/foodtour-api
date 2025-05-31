namespace FoodTour.API.Models
{
    public class MomoPaymentRequest
    {
        public string PartnerCode { get; set; }
        public string AccessKey { get; set; }
        public string RequestId { get; set; }
        public string Amount { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public string ReturnUrl { get; set; }
        public string NotifyUrl { get; set; }
        public string RequestType { get; set; } = "captureWallet";
        public string Signature { get; set; }
        public string Lang { get; set; } = "en";
        public string ExtraData { get; set; }
    }
}
