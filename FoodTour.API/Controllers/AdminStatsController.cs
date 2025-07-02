using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTour.API.Data;

namespace FoodTour.API.Controllers;

[ApiController]
[Route("api/admin/stats")]
public class AdminStatsController : ControllerBase
{
    private readonly FoodTourDbContext _db;

    public AdminStatsController(FoodTourDbContext db)
    {
        _db = db;
    }

    // I. Người dùng
    [HttpGet("users/summary")]
    public async Task<IActionResult> GetUserSummary()
    {
        var total = await _db.Users.CountAsync();
        var newThisMonth = await _db.Users
            .CountAsync(u => u.SubscriptionDate != null &&
                u.SubscriptionDate.Value.Month == DateTime.UtcNow.Month &&
                u.SubscriptionDate.Value.Year == DateTime.UtcNow.Year);
        var paid = await _db.Users.CountAsync(u => u.Role == "Paid");

        return Ok(new { total, newThisMonth, paid });
    }

    [HttpGet("users/list")]
    public async Task<IActionResult> GetUserList()
    {
        var users = await _db.Users
            .Include(u => u.ChatSessions)
            .Include(u => u.SavedRoutes)
            .Include(u => u.Feedbacks)
            .Include(u => u.Posts)
            .ToListAsync();

        var result = users.Select(u => new {
            u.Id,
            u.Email,
            u.Role,
            SubscriptionDate = u.SubscriptionDate?.ToString("yyyy-MM-dd"),
            HasChat = u.ChatSessions?.Any() == true,
            HasSavedRoute = u.SavedRoutes?.Any() == true,
            HasFeedback = u.Feedbacks?.Any() == true,
            HasPost = u.Posts?.Any() == true
        }).ToList();

        return Ok(result);
    }

    [HttpGet("users/active")]
    public async Task<IActionResult> GetActiveUsers()
    {
        var users = await _db.Users
            .Where(u =>
                u.ChatSessions.Any(s => s.ChatMessages.Count > 3)
                || u.SavedRoutes.Any()
                || _db.PaymentTransactions.Any(p => p.OrderId == u.Id.ToString() && p.ResultCode == 0)
            )
            .Select(u => new {
                u.Email,
                u.Role,
                u.SubscriptionDate
            })
            .ToListAsync();

        return Ok(users);
    }

    // II. Chatbot
    [HttpGet("chats/summary")]
    public async Task<IActionResult> GetChatSummary()
    {
        var totalSessions = await _db.ChatSessions.CountAsync();
        var totalMessages = await _db.ChatMessages.CountAsync();
        var avgMessagesPerSession = totalSessions > 0 ? (double)totalMessages / totalSessions : 0;
        var activeSessions = await _db.ChatSessions.CountAsync(s => s.Status == "InProgress");

        return Ok(new { totalSessions, totalMessages, avgMessagesPerSession, activeSessions });
    }

    // III. Route
    [HttpGet("routes/summary")]
    public async Task<IActionResult> GetRouteSummary()
    {
        var totalRoutes = await _db.SavedRoutes.CountAsync();

        var routePlaces = await _db.SavedRoutePlaces.ToListAsync();
        var avgPlacesPerRoute = routePlaces
            .GroupBy(p => p.RouteId)
            .Average(g => g.Count());

        var topPlaces = routePlaces
            .GroupBy(p => p.Name)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new { Place = g.Key, Count = g.Count() })
            .ToList();

        return Ok(new { totalRoutes, avgPlacesPerRoute, topPlaces });
    }

    // IV. Feedback
    [HttpGet("feedbacks/summary")]
    public async Task<IActionResult> GetFeedbackSummary()
    {
        var total = await _db.Feedbacks.CountAsync();
        var withComment = await _db.Feedbacks.CountAsync(f => !string.IsNullOrWhiteSpace(f.Comment));
        var feedbackPerDay = await _db.Feedbacks
            .Where(f => f.CreatedAt != null)
            .GroupBy(f => f.CreatedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new { total, withComment, feedbackPerDay });
    }

    // V. Cộng đồng
    [HttpGet("posts/summary")]
    public async Task<IActionResult> GetPostSummary()
    {
        var totalPosts = await _db.Posts.CountAsync();
        var totalComments = await _db.PostComments.CountAsync();
        var totalLikes = await _db.PostLikes.CountAsync();

        return Ok(new { totalPosts, totalComments, totalLikes });
    }

    // VI. Thanh toán
    [HttpGet("payments/summary")]
    public async Task<IActionResult> GetPaymentSummary()
    {
        var total = await _db.PaymentTransactions.CountAsync();
        var totalSuccess = await _db.PaymentTransactions.CountAsync(p => p.ResultCode == 0);
        var totalFailed = total - totalSuccess;
        var revenue = await _db.PaymentTransactions
            .Where(p => p.ResultCode == 0)
            .SumAsync(p => p.Amount);

        return Ok(new { total, totalSuccess, totalFailed, revenue });
    }

    [HttpGet("payments/list")]
    public async Task<IActionResult> GetPaymentList()
    {
        var payments = await _db.PaymentTransactions
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new {
                p.OrderId,
                p.RequestId,
                p.Amount,
                p.ResultCode,
                p.Message,
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToListAsync();

        return Ok(payments);
    }

    // VII. Timeline hoạt động
    [HttpGet("activity/timeline")]
    public async Task<IActionResult> GetActivityTimeline()
    {
        var chatStats = await _db.ChatSessions
            .Where(s => s.StartedAt != null)
            .GroupBy(s => s.StartedAt!.Value.Date)
            .Select(g => new { Date = g.Key, ChatSessions = g.Count() })
            .ToListAsync();

        var routes = await _db.SavedRoutes
            .Where(r => r.SavedAt != null)
            .GroupBy(r => r.SavedAt!.Value.Date)
            .Select(g => new { Date = g.Key, RoutesSaved = g.Count() })
            .ToListAsync();

        var payments = await _db.PaymentTransactions
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new {
                Date = g.Key,
                Transactions = g.Count(),
                Revenue = g.Sum(x => x.ResultCode == 0 ? x.Amount : 0)
            })
            .ToListAsync();

        return Ok(new { chatStats, routes, payments });
    }

    [HttpGet("users/engaged")]
    public async Task<IActionResult> GetEngagedUsers()
    {
        var users = await _db.Users
            .Include(u => u.ChatSessions)
                .ThenInclude(s => s.ChatMessages)
            .Include(u => u.SavedRoutes)
            .Include(u => u.Feedbacks)
            .Include(u => u.Posts)
            .Include(u => u.PostLikes)
            .Include(u => u.PostComments)
            .ToListAsync();

        var result = users
            .Where(u =>
                u.ChatSessions.Any(s => s.ChatMessages.Count >= 3) ||  // chat thật
                u.SavedRoutes.Any() ||
                u.Feedbacks.Any() ||
                u.Posts.Any() || u.PostLikes.Any() || u.PostComments.Any() ||
                _db.PaymentTransactions.Any(p => p.OrderId == u.Id.ToString() && p.ResultCode == 0)
            )
            .Select(u => new {
                u.Id,
                u.Email,
                u.Role,
                SubscriptionDate = u.SubscriptionDate?.ToString("yyyy-MM-dd"),
                ChatSessions = u.ChatSessions.Count,
                Messages = u.ChatSessions.SelectMany(s => s.ChatMessages).Count(),
                SavedRoutes = u.SavedRoutes.Count,
                Feedbacks = u.Feedbacks.Count,
                Posts = u.Posts.Count,
                Likes = u.PostLikes.Count,
                Comments = u.PostComments.Count
            })
            .ToList();

        return Ok(result);
    }

    [HttpGet("users/activity-log")]
    public async Task<IActionResult> GetUserActivityLog()
    {
        var chatActivities = await _db.ChatSessions
            .Where(s => s.StartedAt != null)
            .Select(s => new {
                Date = s.StartedAt!.Value.Date,
                s.UserId,
                Activity = "Chat"
            }).ToListAsync();

        var routeActivities = await _db.SavedRoutes
            .Where(r => r.SavedAt != null)
            .Select(r => new {
                Date = r.SavedAt!.Value.Date,
                r.UserId,
                Activity = "SavedRoute"
            }).ToListAsync();

        var postActivities = await _db.Posts
            .Select(p => new {
                Date = p.CreatedAt.Date,
                p.UserId,
                Activity = "Post"
            }).ToListAsync();

        var commentActivities = await _db.PostComments
            .Select(c => new {
                Date = c.CreatedAt.Date,
                c.UserId,
                Activity = "Comment"
            }).ToListAsync();

        var feedbackActivities = await _db.Feedbacks
            .Where(f => f.CreatedAt != null && f.UserId != null)
            .Select(f => new {
                Date = f.CreatedAt!.Value.Date,
                UserId = f.UserId!.Value,
                Activity = "Feedback"
            }).ToListAsync();

        var all = chatActivities
            .Concat(routeActivities)
            .Concat(postActivities)
            .Concat(commentActivities)
            .Concat(feedbackActivities)
            .ToList();

        var userEmails = await _db.Users.ToDictionaryAsync(u => u.Id, u => u.Email);

        var result = all
            .Where(x => userEmails.ContainsKey(x.UserId))
            .Select(x => new {
                x.Date,
                Email = userEmails[x.UserId],
                x.Activity
            })
            .OrderBy(x => x.Date)
            .ToList();

        return Ok(result);
    }

    [HttpGet("users/activity-summary")]
    public async Task<IActionResult> GetUserActivitySummary()
    {
        var chat = await _db.ChatSessions
            .Where(s => s.StartedAt != null)
            .GroupBy(s => new { Date = s.StartedAt!.Value.Date, Activity = "Chat" })
            .Select(g => new {
                g.Key.Date,
                g.Key.Activity,
                UserCount = g.Select(x => x.UserId).Distinct().Count()
            }).ToListAsync();

        var routes = await _db.SavedRoutes
            .Where(r => r.SavedAt != null)
            .GroupBy(r => new { Date = r.SavedAt!.Value.Date, Activity = "SavedRoute" })
            .Select(g => new {
                g.Key.Date,
                g.Key.Activity,
                UserCount = g.Select(x => x.UserId).Distinct().Count()
            }).ToListAsync();

        var posts = await _db.Posts
            .GroupBy(p => new { Date = p.CreatedAt.Date, Activity = "Post" })
            .Select(g => new {
                g.Key.Date,
                g.Key.Activity,
                UserCount = g.Select(x => x.UserId).Distinct().Count()
            }).ToListAsync();

        var comments = await _db.PostComments
            .GroupBy(c => new { Date = c.CreatedAt.Date, Activity = "Comment" })
            .Select(g => new {
                g.Key.Date,
                g.Key.Activity,
                UserCount = g.Select(x => x.UserId).Distinct().Count()
            }).ToListAsync();

        var feedback = await _db.Feedbacks
    .Where(f => f.CreatedAt != null && f.UserId != null)
    .GroupBy(f => new { Date = f.CreatedAt!.Value.Date, Activity = "Feedback" })
    .Select(g => new {
        g.Key.Date,
        g.Key.Activity,
        UserCount = g.Select(x => x.UserId!.Value).Distinct().Count()
    }).ToListAsync();

        var combined = chat
            .Concat(routes)
            .Concat(posts)
            .Concat(comments)
            .Concat(feedback)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Activity)
            .ToList();

        return Ok(combined);
    }
}
