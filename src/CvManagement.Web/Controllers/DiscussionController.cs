using CvManagement.Domain.Entities;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Hubs;
using CvManagement.Web.Models.Discussions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize]
public class DiscussionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<DiscussionHub> _hubContext;

    public DiscussionController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IHubContext<DiscussionHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts(Guid positionId)
    {
        var posts = await _context.DiscussionPosts
            .Where(p => p.PositionId == positionId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

        var userIds = posts.Select(p => p.AuthorUserId).Distinct().ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var userDict = users.ToDictionary(u => u.Id);

        var profileIds = users
            .Where(u => u.ProfileId.HasValue)
            .Select(u => u.ProfileId!.Value)
            .ToList();
        var profiles = await _context.Profiles
            .Where(p => profileIds.Contains(p.Id))
            .ToListAsync();

        var profileDict = profiles.ToDictionary(p => p.Id);

        var viewModels = posts.Select(p =>
        {
            string authorName;
            Guid? authorProfileId = null;
            string? authorPhotoUrl = null;

            if (userDict.TryGetValue(p.AuthorUserId, out var user))
            {
                authorProfileId = user.ProfileId;
                if (user.ProfileId.HasValue && profileDict.TryGetValue(user.ProfileId.Value, out var profile))
                {
                    authorName = $"{profile.FirstName} {profile.LastName}";
                    authorPhotoUrl = profile.PhotoUrl;
                }
                else
                    authorName = user.Email ?? "Unknown";
            }
            else
            {
                authorName = "Unknown";
            }

            return new DiscussionPostViewModel
            {
                Id = p.Id,
                AuthorName = authorName,
                AuthorProfileId = authorProfileId,
                AuthorPhotoUrl = authorPhotoUrl,
                Text = p.Text,
                CreatedAt = p.CreatedAt
            };
        }).ToList();

        return Ok(viewModels);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreateDiscussionPostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Text is required");

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return BadRequest("User not found");

        var positionExists = await _context.Positions.AnyAsync(p => p.Id == request.PositionId);
        if (!positionExists)
            return NotFound();

        var entity = new DiscussionPost
        {
            PositionId = request.PositionId,
            AuthorUserId = user.Id,
            Text = request.Text,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.DiscussionPosts.Add(entity);
        await _context.SaveChangesAsync();

        string authorName;
        string? authorPhotoUrl = null;
        if (user.ProfileId.HasValue)
        {
            var profile = await _context.Profiles.FindAsync(user.ProfileId.Value);
            if (profile is not null)
            {
                authorName = $"{profile.FirstName} {profile.LastName}";
                authorPhotoUrl = profile.PhotoUrl;
            }
            else
            {
                authorName = user.Email ?? "Unknown";
            }
        }
        else
        {
            authorName = user.Email ?? "Unknown";
        }

        await _hubContext.Clients
            .Group($"position-{request.PositionId}")
            .SendAsync("ReceivePost", new
            {
                id = entity.Id,
                authorName,
                authorProfileId = user.ProfileId,
                authorPhotoUrl,
                text = entity.Text,
                createdAt = entity.CreatedAt
            });

        return Ok();
    }
}
