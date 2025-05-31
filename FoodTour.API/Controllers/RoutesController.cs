using FoodTour.API.Data;
using FoodTour.API.DTOs;
using FoodTour.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodTour.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        private readonly FoodTourDbContext _db;

        public RoutesController(FoodTourDbContext db)
        {
            _db = db;
        }

        // POST: api/routes/save
        [HttpPost("save")]
        [Authorize]
        public async Task<IActionResult> SaveConfirmedRoute([FromBody] ConfirmRouteDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var session = await _db.ChatSessions.FindAsync(dto.SessionId);
            if (session == null || session.UserId != userId)
                return BadRequest("Session không hợp lệ");

            var route = new SavedRoute
            {
                Id = Guid.NewGuid(),
                SessionId = dto.SessionId,
                UserId = userId,
                Name = dto.RouteName,
                Description = dto.Description,
                SavedAt = DateTime.UtcNow
            };

            _db.SavedRoutes.Add(route);

            int seq = 1;
            foreach (var place in dto.Places)
            {
                _db.SavedRoutePlaces.Add(new SavedRoutePlace
                {
                    RouteId = route.Id,
                    Name = place.Name,
                    Address = place.Address,
                    Lat = place.Lat,
                    Lng = place.Lng,
                    TimeSlot = place.TimeSlot,
                    Role = place.Role,
                    Note = place.Note,
                    Sequence = seq++
                });
            }

            session.Status = "Saved";
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đã lưu lộ trình thành công!" });
        }

        // GET: api/routes/history
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetSavedRoutes()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var routes = await _db.SavedRoutes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.SavedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.SavedAt,
                    Places = _db.SavedRoutePlaces
                        .Where(p => p.RouteId == r.Id)
                        .OrderBy(p => p.Sequence)
                        .Select(p => new
                        {
                            p.Name,
                            p.Address,
                            p.TimeSlot,
                            p.Role,
                            p.Note,
                            p.Lat,
                            p.Lng
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(routes);
        }

        // GET: api/routes/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetRouteDetail(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var route = await _db.SavedRoutes
                .Where(r => r.Id == id && r.UserId == userId)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    r.SavedAt,
                    SessionId = r.SessionId,
                    Places = _db.SavedRoutePlaces
                        .Where(p => p.RouteId == r.Id)
                        .OrderBy(p => p.Sequence)
                        .Select(p => new
                        {
                            p.Name,
                            p.Address,
                            p.Lat,
                            p.Lng,
                            p.TimeSlot,
                            p.Role,
                            p.Note,
                            p.Sequence
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (route == null)
                return NotFound("Không tìm thấy lộ trình.");

            return Ok(route);
        }

        // PUT: api/routes/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRouteInfo(Guid id, [FromBody] UpdateRouteInfoDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var route = await _db.SavedRoutes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            if (route == null)
                return NotFound("Không tìm thấy lộ trình.");

            route.Name = dto.Name;
            route.Description = dto.Description;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật lộ trình thành công." });
        }

        // POST: api/routes/save-default
        [HttpPost("save-default")]
        [Authorize]
        public async Task<IActionResult> SaveDefaultRoute([FromBody] DefaultRouteDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var route = new SavedRoute
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                SavedAt = DateTime.UtcNow
            };

            _db.SavedRoutes.Add(route);

            int seq = 1;
            foreach (var place in dto.Places)
            {
                _db.SavedRoutePlaces.Add(new SavedRoutePlace
                {
                    RouteId = route.Id,
                    Name = place.Name,
                    Address = place.Address,
                    Sequence = seq++,
                    Lat = place.Lat,
                    Lng = place.Lng,
                    Note = place.Note,
                    Role = place.Role,
                    TimeSlot = place.TimeSlot
                });
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã lưu lộ trình mặc định thành công!" });
        }

    }
}
