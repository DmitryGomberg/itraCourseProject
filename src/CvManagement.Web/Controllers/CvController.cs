using CvManagement.Domain;
using CvManagement.Domain.Entities;
using CvManagement.Domain.Services;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Models.Cv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize]
public class CvController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CvController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<Guid?> GetCurrentProfileId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.ProfileId;
    }

    private bool AllAttributesFilled(Profile profile, Position position)
    {
        var avDict = profile.AttributeValues.ToDictionary(av => av.AttributeDefinitionId);
        foreach (var pa in position.PositionAttributes)
        {
            if (!avDict.TryGetValue(pa.AttributeDefinitionId, out var av))
                return false;

            var isEmpty = av.StringValue is null && av.TextValue is null && !av.NumericValue.HasValue
                && !av.DateValue.HasValue && !av.BooleanValue.HasValue && !av.SelectedOptionId.HasValue;
            if (isEmpty)
                return false;
        }
        return true;
    }

    private List<CvProjectViewModel> GetFilteredProjects(Profile profileWithProjectsAndTags, Position position)
    {
        var relevantTagNames = position.RelevantProjectTags
            .Select(t => t.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        IEnumerable<Project> filtered;
        if (relevantTagNames.Count == 0)
            filtered = profileWithProjectsAndTags.Projects;
        else
            filtered = profileWithProjectsAndTags.Projects
                .Where(p => p.Tags.Any(t => relevantTagNames.Contains(t.Name)));

        return filtered
            .OrderByDescending(p => p.PeriodStart)
            .Take(position.MaxProjects)
            .Select(p => new CvProjectViewModel
            {
                Name = p.Name,
                PeriodDisplay = p.PeriodEnd.HasValue
                    ? $"{p.PeriodStart:yyyy-MM} — {p.PeriodEnd.Value:yyyy-MM}"
                    : $"{p.PeriodStart:yyyy-MM} — present",
                DescriptionMarkdown = p.DescriptionMarkdown,
                Tags = p.Tags.Select(t => t.Name).ToList()
            })
            .ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid positionId)
    {
        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var profile = await _context.Profiles
            .Include(p => p.AttributeValues)
                .ThenInclude(v => v.SelectedOption)
            .FirstOrDefaultAsync(p => p.Id == profileId.Value);

        if (profile is null)
            return BadRequest("Profile not found");

        var position = await _context.Positions
            .Include(p => p.AccessRules)
            .FirstOrDefaultAsync(p => p.Id == positionId);

        if (position is null)
            return NotFound();

        if (!PositionAccessEvaluator.IsAllowed(profile, position))
        {
            TempData["Error"] = "У вас нет доступа к этой позиции";
            return RedirectToAction(nameof(BrowsePositionsController.Index), "BrowsePositions");
        }

        var existingCv = await _context.Cvs
            .FirstOrDefaultAsync(c => c.ProfileId == profile.Id && c.PositionId == positionId);

        if (existingCv is not null)
            return RedirectToAction(nameof(Details), new { id = existingCv.Id });

        var entity = new Cv
        {
            ProfileId = profile.Id,
            PositionId = positionId,
            Status = CvStatus.Draft
        };

        _context.Cvs.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var currentUserProfileId = await GetCurrentProfileId();
        if (currentUserProfileId is null)
            return BadRequest("Profile not found");

        var cv = await _context.Cvs
            .Include(c => c.Position)
                .ThenInclude(p => p.PositionAttributes)
                    .ThenInclude(pa => pa.AttributeDefinition)
                        .ThenInclude(ad => ad.Options)
            .Include(c => c.Position)
                .ThenInclude(p => p.RelevantProjectTags)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cv is null)
            return NotFound();

        var profile = await _context.Profiles
            .Include(p => p.AttributeValues)
                .ThenInclude(v => v.SelectedOption)
            .Include(p => p.Projects)
                .ThenInclude(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == cv.ProfileId);

        if (profile is null)
            return NotFound();

        var isOwner = currentUserProfileId.Value == cv.ProfileId;

        if (!isOwner)
        {
            if (!User.IsInRole("Recruiter") && !User.IsInRole("Administrator"))
                return Forbid();
            if (cv.Status != CvStatus.Published)
                return Forbid();
        }

        var avDict = profile.AttributeValues.ToDictionary(av => av.AttributeDefinitionId);

        var attributes = cv.Position.PositionAttributes
            .OrderBy(pa => pa.DisplayOrder)
            .Select(pa =>
            {
                var av = avDict.TryGetValue(pa.AttributeDefinitionId, out var val) ? val : null;
                var isEmpty = av is null || (
                    av.StringValue is null && av.TextValue is null && !av.NumericValue.HasValue
                    && !av.DateValue.HasValue && !av.BooleanValue.HasValue && !av.SelectedOptionId.HasValue);

                var options = pa.AttributeDefinition.Options
                    .Select(o => (o.Id, o.Value))
                    .ToList();

                return new CvAttributeFieldViewModel
                {
                    AttributeDefinitionId = pa.AttributeDefinitionId,
                    Name = pa.AttributeDefinition.Name,
                    DataType = pa.AttributeDefinition.DataType.ToString(),
                    StringValue = av?.StringValue,
                    TextValue = av?.TextValue,
                    NumericValue = av?.NumericValue,
                    DateValue = av?.DateValue,
                    PeriodStart = av?.PeriodStart,
                    PeriodEnd = av?.PeriodEnd,
                    BooleanValue = av?.BooleanValue,
                    SelectedOptionId = av?.SelectedOptionId,
                    Options = options,
                    IsEmpty = isEmpty
                };
            })
            .ToList();

        var projects = GetFilteredProjects(profile, cv.Position);

        var canEdit = isOwner && cv.Status == CvStatus.Draft;
        var canPublish = canEdit && AllAttributesFilled(profile, cv.Position);
        var canUnpublish = isOwner && cv.Status == CvStatus.Published;

        var likesCount = await _context.CvLikes.CountAsync(l => l.CvId == cv.Id);

        var canLike = !isOwner && (User.IsInRole("Recruiter") || User.IsInRole("Administrator"))
            && cv.Status == CvStatus.Published;

        var likedByCurrentUser = false;
        if (canLike)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is not null)
            {
                likedByCurrentUser = await _context.CvLikes
                    .AnyAsync(l => l.CvId == cv.Id && l.RecruiterUserId == user.Id);
            }
        }

        var viewModel = new CvSummaryViewModel
        {
            Id = cv.Id,
            PositionId = cv.PositionId,
            PositionTitle = cv.Position.Title,
            PositionShortDescription = cv.Position.ShortDescription,
            Status = cv.Status.ToString(),
            IsDraft = cv.Status == CvStatus.Draft,
            CanEdit = canEdit,
            CanPublish = canPublish,
            CanUnpublish = canUnpublish,
            Attributes = attributes,
            Projects = projects,
            Version = cv.Version,
            LikesCount = likesCount,
            LikedByCurrentUser = likedByCurrentUser,
            CanLike = canLike
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(Guid id, uint version)
    {
        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var cv = await _context.Cvs.FindAsync(id);
        if (cv is null)
            return NotFound();

        if (cv.ProfileId != profileId.Value)
            return Forbid();

        var profile = await _context.Profiles
            .Include(p => p.AttributeValues)
            .FirstOrDefaultAsync(p => p.Id == profileId.Value);

        var position = await _context.Positions
            .Include(p => p.PositionAttributes)
            .FirstOrDefaultAsync(p => p.Id == cv.PositionId);

        if (profile is null || position is null)
            return NotFound();

        if (!AllAttributesFilled(profile, position))
        {
            TempData["Error"] = "Заполните все атрибуты перед публикацией";
            return RedirectToAction(nameof(Details), new { id });
        }

        _context.Entry(cv).Property("Version").OriginalValue = version;
        cv.Status = CvStatus.Published;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "CV был изменён, обновите страницу";
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(Guid id, uint version)
    {
        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var cv = await _context.Cvs.FindAsync(id);
        if (cv is null)
            return NotFound();

        if (cv.ProfileId != profileId.Value)
            return Forbid();

        _context.Entry(cv).Property("Version").OriginalValue = version;
        cv.Status = CvStatus.Draft;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "CV был изменён, обновите страницу";
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Recruiter,Administrator")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return BadRequest("User not found");

        var cv = await _context.Cvs.FindAsync(id);
        if (cv is null)
            return NotFound();

        if (cv.Status != CvStatus.Published)
            return Forbid();

        var existingLike = await _context.CvLikes
            .FirstOrDefaultAsync(l => l.CvId == id && l.RecruiterUserId == user.Id);

        if (existingLike is not null)
        {
            _context.CvLikes.Remove(existingLike);
        }
        else
        {
            var newLike = new CvLike
            {
                CvId = id,
                RecruiterUserId = user.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.CvLikes.Add(newLike);
            _context.Entry(newLike).State = EntityState.Added;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }
}
