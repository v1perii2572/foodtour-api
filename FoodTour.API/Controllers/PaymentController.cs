using FoodTour.API.DTOs;
using FoodTour.API.Models;
using FoodTour.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FoodTour.API.Data;
using System.Security.Claims;

namespace FoodTour.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly MomoService _momoService;
        private readonly ILogger<PaymentController> _logger;
        private readonly FoodTourDbContext _context;

        public PaymentController(
            MomoService momoService,
            ILogger<PaymentController> logger,
            FoodTourDbContext context)
        {
            _momoService = momoService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("link")]
        public async Task<IActionResult> GetPaymentLink(string amount, string extraData)
        {
            if (!int.TryParse(amount, out int amountValue) || amountValue < 1000 || amountValue > 50000000)
                return BadRequest("Số tiền không hợp lệ.");

            var payUrl = await _momoService.CreatePaymentAsync(amount, extraData);
            if (string.IsNullOrEmpty(payUrl))
                return BadRequest("Không tạo được liên kết thanh toán.");

            return Ok(new { payUrl });
        }

        [HttpGet("create")]
        public async Task<IActionResult> CreateAndRedirect(string amount, string extraData)
        {
            var payUrl = await _momoService.CreatePaymentAsync(amount, extraData);
            if (string.IsNullOrEmpty(payUrl))
                return BadRequest("Không tạo được liên kết thanh toán.");

            return Redirect(payUrl);
        }

        [HttpGet("return")]
        public IActionResult MomoReturn([FromQuery] MomoReturnDto query)
        {
            _logger.LogInformation("MoMo Return: OrderId={OrderId}, ResultCode={ResultCode}, Message={Message}",
                query.OrderId, query.ResultCode, query.Message);

            var transaction = new PaymentTransaction
            {
                OrderId = query.OrderId,
                RequestId = query.RequestId,
                Amount = int.TryParse(query.Amount, out int amt) ? amt : 0,
                ResultCode = int.TryParse(query.ResultCode, out int rc) ? rc : -1,
                Message = query.Message,
                CreatedAt = DateTime.Now
            };

            _context.PaymentTransactions.Add(transaction);

            if (transaction.ResultCode == 0 && !string.IsNullOrEmpty(query.ExtraData))
            {
                var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == query.ExtraData);
                if (user != null)
                {
                    int extraDays = transaction.Amount switch
                    {
                        49000 => 7,
                        129000 => 30,
                        549000 => 180,
                        849000 => 365,
                        _ => 30
                    };

                    user.Role = transaction.Amount == 849000 ? "SVIP" : "VIP";

                    var now = DateTime.Now;
                    var currentDate = user.SubscriptionDate ?? now;
                    user.SubscriptionDate = currentDate > now
                        ? currentDate.AddDays(extraDays)
                        : now.AddDays(extraDays);
                }
            }

            _context.SaveChanges();

            return Redirect("http://localhost:5173/trang-ca-nhan");
        }

        [HttpPost("notify")]
        public async Task<IActionResult> MomoNotify([FromBody] MomoNotifyDto notify)
        {
            _logger.LogInformation("MoMo Notify: OrderId={OrderId}, ResultCode={ResultCode}, Message={Message}",
                notify.OrderId, notify.ResultCode, notify.Message);

            var transaction = new PaymentTransaction
            {
                OrderId = notify.OrderId,
                RequestId = notify.RequestId,
                Amount = int.TryParse(notify.Amount, out int amt) ? amt : 0,
                ResultCode = notify.ResultCode,
                Message = notify.Message,
                CreatedAt = DateTime.Now
            };

            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notify received and saved." });
        }
    }
}
