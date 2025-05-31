using FoodTour.API.Data;
using FoodTour.API.DTOs;
using FoodTour.API.Models;
using FoodTour.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTour.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FoodTourDbContext _db;
        private readonly TokenService _tokenService;

        public AuthController(FoodTourDbContext db, TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (_db.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email đã được sử dụng");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Free"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("Đăng ký thành công");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Thông tin đăng nhập không đúng");

            var token = _tokenService.GenerateToken(user);
            return Ok(new
            {
                token = token,
                user = new
                {
                    name = user.Name,
                    email = user.Email
                }
            });
        }
    }
}
