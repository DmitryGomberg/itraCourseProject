using CvManagement.Domain.Services;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Models.CandidatePositions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize(Roles = "Candidate,Administrator")]
public class BrowsePositionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BrowsePositionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.ProfileId is null)
            return BadRequest("Profile not found");

        var profile = await _context.Profiles
            .Include(p => p.AttributeValues)
                .ThenInclude(v => v.SelectedOption)
            .FirstOrDefaultAsync(p => p.Id == user.ProfileId.Value);

        if (profile is null)
            return BadRequest("Profile not found");

        var positions = await _context.Positions
            .Where(p => p.IsPublic)
            .Include(p => p.AccessRules)
            .ToListAsync();

        var allowedPositions = positions
            .Where(p => PositionAccessEvaluator.IsAllowed(profile, p))
            .ToList();

        var positionIds = allowedPositions.Select(p => p.Id).ToList();

        var existingCvs = await _context.Cvs
            .Where(c => c.ProfileId == profile.Id && positionIds.Contains(c.PositionId))
            .ToListAsync();

        var cvDict = existingCvs.ToDictionary(c => c.PositionId);

        var viewModels = allowedPositions.Select(p =>
        {
            cvDict.TryGetValue(p.Id, out var cv);
            return new AvailablePositionViewModel
            {
                Id = p.Id,
                Title = p.Title,
                ShortDescription = p.ShortDescription,
                ExistingCvId = cv?.Id,
                ExistingCvStatus = cv?.Status.ToString()
            };
        }).ToList();

        return View(viewModels);
    }
}
