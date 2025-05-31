using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FoodTour.API.Data;
using FoodTour.API.Models;
using FoodTour.API.DTOs;

namespace FoodTour.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly FoodTourDbContext _context;

    public UserController(FoodTourDbContext context)
    {
        _context = context;
    }

    // GET: api/user/profile
    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Name,
            user.Role,
            user.SubscriptionDate,
            DislikedFoods = user.DislikedFoods?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? new string[0]
        });
    }

    // PUT: api/user/profile
    [HttpPut("profile")]
    [Authorize]
    public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var user = _context.Users.FirstOrDefault(u => u.Id.ToString() == userId);
        if (user == null) return NotFound();

        user.Name = request.Name;
        user.DislikedFoods = string.Join(';', request.DislikedFoods);

        _context.SaveChanges();

        return Ok(new { message = "Profile updated successfully." });
    }
}