using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using FoodTour.API.Models;
using Microsoft.Extensions.Options;

namespace FoodTour.API.Services
{
    public class MomoService
    {
        private readonly MomoSettings _settings;

        public MomoService(IOptions<MomoSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<string?> CreatePaymentAsync(string amount, string extraData)
        {
            string orderId = $"EA{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            string requestId = Guid.NewGuid().ToString();
            string encodedExtraData = Uri.EscapeDataString(extraData ?? "");

            string rawHash = $"accessKey={_settings.AccessKey}" +
                             $"&amount={amount}" +
                             $"&extraData={encodedExtraData}" +
                             $"&ipnUrl={_settings.NotifyUrl}" +
                             $"&orderId={orderId}" +
                             $"&orderInfo=Eat Around" +
                             $"&partnerCode={_settings.PartnerCode}" +
                             $"&redirectUrl={_settings.ReturnUrl}" +
                             $"&requestId={requestId}" +
                             $"&requestType=captureWallet";

            string signature = GenerateSignature(rawHash, _settings.SecretKey);

            var request = new
            {
                partnerCode = _settings.PartnerCode,
                accessKey = _settings.AccessKey,
                requestId,
                amount,
                orderId,
                orderInfo = "Eat Around",
                redirectUrl = _settings.ReturnUrl,
                ipnUrl = _settings.NotifyUrl,
                requestType = "captureWallet",
                extraData = encodedExtraData,
                signature,
                lang = "vi"
            };

            var client = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_settings.Endpoint, content);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("MoMo JSON Response: " + json);

            dynamic result = JsonConvert.DeserializeObject(json);
            if (result == null || result.resultCode != 0)
            {
                Console.WriteLine($"MoMo ERROR: resultCode={result?.resultCode}, message={result?.message}");
                return null;
            }

            return result.payUrl;
        }

        private string GenerateSignature(string rawData, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
