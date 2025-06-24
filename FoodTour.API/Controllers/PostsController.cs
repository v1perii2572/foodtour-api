using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTour.API.Data;
using FoodTour.API.DTOs;
using FoodTour.API.Models;

namespace FoodTour.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly FoodTourDbContext _context;

        public PostsController(FoodTourDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                Images = dto.ImageUrls.Select(url => new PostImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = url
                }).ToList()
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { post.Id });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts([FromQuery] Guid userId)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    UserName = p.User.Name,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),
                    LikeCount = p.Likes.Count,
                    IsLiked = p.Likes.Any(l => l.UserId == userId),
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        UserName = c.User.Name,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(Guid id, [FromQuery] Guid userId)
        {
            var existingLike = await _context.PostLikes.FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId);

            if (existingLike != null)
            {
                _context.PostLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(new { liked = false });
            }
            else
            {
                var like = new PostLike
                {
                    Id = Guid.NewGuid(),
                    PostId = id,
                    UserId = userId,
                    LikedAt = DateTime.UtcNow
                };
                _context.PostLikes.Add(like);
                await _context.SaveChangesAsync();
                return Ok(new { liked = true });
            }
        }

        [HttpPost("{id}/comment")]
        public async Task<IActionResult> Comment(Guid id, [FromBody] CreateCommentDto dto)
        {
            var comment = new PostComment
            {
                Id = Guid.NewGuid(),
                PostId = id,
                UserId = dto.UserId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(Guid id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .Where(p => p.Id == id)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    UserName = p.User.Name,
                    CreatedAt = p.CreatedAt,
                    ImageUrls = p.Images.Select(i => i.ImageUrl).ToList(),
                    LikeCount = p.Likes.Count,
                    IsLiked = false,
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        UserName = c.User.Name,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (post == null) return NotFound();
            return Ok(post);
        }
    }
}
