using FoodTour.API.Data;
using FoodTour.API.DTOs;
using FoodTour.API.Models;
using FoodTour.API.Services;
using FoodTour.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace FoodTour.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly FoodTourDbContext _db;
        private readonly CohereAIService _cohere;
        private readonly GoogleGeocodingService _geo;
        private readonly GeminiService _gemini;

        public ChatController(FoodTourDbContext db, CohereAIService cohere, GoogleGeocodingService geo, GeminiService gemini)
        {
            _db = db;
            _cohere = cohere;
            _geo = geo;
            _gemini = gemini;
        }

        private string ExtractTimeRange(string message)
        {
            message = message.ToLower();
            if (message.Contains("sáng")) return "buổi sáng";
            if (message.Contains("trưa")) return "buổi trưa";
            if (message.Contains("tối")) return "buổi tối";
            if (message.Contains("mai")) return "ngày mai";
            if (message.Contains("nay")) return "ngày hôm nay";
            return "cả ngày";
        }

        [HttpPost("message")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            ChatSession session;
            if (dto.SessionId == null)
            {
                session = new ChatSession
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StartedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    Status = "InProgress"
                };
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();
            }
            else
            {
                session = await _db.ChatSessions.FindAsync(dto.SessionId);
                if (session == null || session.UserId != userId)
                    return BadRequest("Session không hợp lệ");
            }

            var userMsg = new ChatMessage
            {
                SessionId = session.Id,
                Role = "user",
                Message = dto.Message,
                Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            _db.ChatMessages.Add(userMsg);
            await _db.SaveChangesAsync();

            var history = _db.ChatMessages
                .Where(m => m.SessionId == session.Id)
                .OrderBy(m => m.Timestamp)
                .ToList();

            var alreadySuggestedPlaces = _db.ChatMessages
                .Where(m => m.SessionId == session.Id && m.Role == "assistant" && m.Message.Contains("<Tên quán>"))
                .Select(m => m.Message)
                .ToList();

            var savedPlaces = await _db.SavedRoutePlaces
                .Where(rp => rp.Route.UserId == userId)
                .Select(rp => rp.Name)
                .ToListAsync();

            var allSuggestedPlaces = alreadySuggestedPlaces.Concat(savedPlaces).Distinct().ToList();

            var dislikedFoods = new List<string>();
            foreach (var msg in history.Where(m => m.Role == "user"))
            {
                var match = Regex.Match(msg.Message, @"không muốn ăn\s+(.+?)(\.|$)", RegexOptions.IgnoreCase);
                if (match.Success) dislikedFoods.Add(match.Groups[1].Value.Trim());
            }

            if (dto.Message.ToLower().Contains("lưu lộ trình"))
            {
                string aiReply;
                var lastUserIndex = history.FindLastIndex(m => m.Role == "user" && m.Message.ToLower().Contains("lưu lộ trình"));

                if (lastUserIndex > 0)
                {
                    aiReply = history
                        .Take(lastUserIndex)
                        .Reverse()
                        .FirstOrDefault(m => m.Role == "assistant")?.Message ?? "";
                }
                else
                {
                    aiReply = history.LastOrDefault(m => m.Role == "assistant")?.Message ?? "";
                }

                if (string.IsNullOrWhiteSpace(aiReply))
                    return BadRequest("Không có phản hồi AI gần nhất để phân tích.");

                var allPlaces = AIResponseParser.ParseRouteFromAiResponse(aiReply);

                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("Vui lòng gợi ý lộ trình ăn uống mới, tránh những quán đã được gợi ý gần đây trong cuộc trò chuyện này.");
                promptBuilder.AppendLine("Nếu có thể, hãy hạn chế gợi ý những quán ăn đã được gợi ý hoặc lưu trong các lộ trình trước đó.");

                if (dislikedFoods.Any())
                {
                    promptBuilder.AppendLine($"Người dùng không muốn ăn: {string.Join(", ", dislikedFoods)}. Vui lòng tránh các món này.");
                }

                foreach (var msg in history)
                {
                    if (msg.Role == "user")
                        promptBuilder.AppendLine($"User: {msg.Message}");
                    else
                        promptBuilder.AppendLine($"AI: {msg.Message}");
                }

                string aiReplyNew;
                try
                {
                    aiReplyNew = await _gemini.GetSuggestionAsync(promptBuilder.ToString());
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"❌ Lỗi từ Gemini: {ex.Message}");
                }


                var aiMsg = new ChatMessage
                {
                    SessionId = session.Id,
                    Role = "assistant",
                    Message = aiReplyNew,
                    Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };
                _db.ChatMessages.Add(aiMsg);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    sessionId = session.Id,
                    reply = aiReplyNew
                });
            }
            else
            {
                string userAddress = "Hà Nội";
                try
                {
                    userAddress = await _geo.ReverseGeocodeAsync(dto.Lat, dto.Lng);
                }
                catch
                {
                    userAddress = "Hà Nội";
                }

                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine($"Tôi đang ở gần địa chỉ: {userAddress}.");
                promptBuilder.AppendLine($"Tọa độ của tôi là lat {dto.Lat}, lng {dto.Lng}.");
                promptBuilder.AppendLine("Vui lòng chỉ gợi ý các địa điểm ăn uống nằm trong vòng 5km quanh vị trí này.");

                var timeRange = ExtractTimeRange(dto.Message);
                promptBuilder.AppendLine($"Bạn hãy gợi ý lộ trình ăn uống chỉ trong {timeRange}.");
                promptBuilder.AppendLine("Không gợi ý nhiều hơn 1 món chính, 1 món tráng miệng, hoặc 1 quán nước.");
                promptBuilder.AppendLine("Bạn chỉ được phép trả lời theo định dạng sau:");
                promptBuilder.AppendLine("Địa chỉ phải đầy đủ, bao gồm số nhà, tên phố, quận và thành phố Hà Nội. ví dụ: 29 Phố Hàng Than, Quận Ba Đình, Thành Phố Hà Nội");
                promptBuilder.AppendLine("<số thứ tự>. <Giờ> - <Tên quán> (<Địa chỉ>) – <Vai trò> – <Ghi chú>");
                promptBuilder.AppendLine("Không trả lời ngoài định dạng này.");
                promptBuilder.AppendLine("Mỗi địa điểm trên một dòng, không có khoảng cách dòng giữa các địa điểm.");

                var disliked = new List<string>();
                foreach (var msg in history.Where(m => m.Role == "user"))
                {
                    var match = Regex.Match(msg.Message, @"không muốn ăn\s+(.+?)(\.|$)", RegexOptions.IgnoreCase);
                    if (match.Success) disliked.Add(match.Groups[1].Value.Trim());
                }
                if (disliked.Any())
                {
                    promptBuilder.AppendLine($"Người dùng không muốn ăn: {string.Join(", ", disliked)}. Vui lòng không gợi ý các món này.");
                }

                foreach (var msg in history)
                {
                    if (msg.Role == "user")
                        promptBuilder.AppendLine($"User: {msg.Message}");
                    else
                        promptBuilder.AppendLine($"AI: {msg.Message}");
                }

                string aiReplyNew;
                try
                {
                    aiReplyNew = await _gemini.GetSuggestionAsync(promptBuilder.ToString());
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"❌ Lỗi từ Gemini: {ex.Message}");
                }


                var aiMsg = new ChatMessage
                {
                    SessionId = session.Id,
                    Role = "assistant",
                    Message = aiReplyNew,
                    Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };
                _db.ChatMessages.Add(aiMsg);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    sessionId = session.Id,
                    reply = aiReplyNew
                });
            }
        }

        [HttpPost("save-route")]
        [Authorize]
        public async Task<IActionResult> SaveRoute([FromBody] SaveRouteDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var session = await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId && s.UserId == userId);

            if (session == null)
                return BadRequest("Không tìm thấy session");

            var messages = await _db.ChatMessages
                .Where(m => m.SessionId == session.Id)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            var lastAiMsg = messages.FirstOrDefault(m => m.Role == "assistant");

            if (lastAiMsg == null)
                return BadRequest("Không có phản hồi AI để lưu.");

            var allPlaces = AIResponseParser.ParseRouteFromAiResponse(lastAiMsg.Message);
            if (!allPlaces.Any())
                return BadRequest("Không tìm thấy lộ trình trong phản hồi AI.");

            var route = new SavedRoute
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                UserId = userId,
                Name = dto.CustomName ?? $"Lộ trình lúc {DateTime.Now:HH:mm:ss}",
                Description = "Lưu từ phản hồi AI qua nút bấm",
                SavedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            _db.SavedRoutes.Add(route);

            int seq = 1;
            foreach (var p in allPlaces)
            {
                double lat = p.Lat, lng = p.Lng;
                if (lat == 0 || lng == 0)
                {
                    try
                    {
                        var fullAddr = $"{p.Address}, Hà Nội, Việt Nam";
                        (lat, lng) = await _geo.GetLatLngAsync(fullAddr);
                    }
                    catch { lat = lng = 0; }
                }

                _db.SavedRoutePlaces.Add(new SavedRoutePlace
                {
                    RouteId = route.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Lat = lat,
                    Lng = lng,
                    TimeSlot = p.TimeSlot,
                    Role = p.Role,
                    Note = p.Note,
                    Sequence = seq++
                });
            }

            session.Status = "Saved";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đã lưu lộ trình thành công!" });
        }
    }
}
