using CvManagement.Domain;
using CvManagement.Domain.Entities;
using CvManagement.Domain.Services;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Models.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SearchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? tag)
    {
        var viewModel = new SearchResultsViewModel { Query = q ?? "", Tag = tag };

        var term = q?.Trim();
        if (string.IsNullOrWhiteSpace(term) && string.IsNullOrWhiteSpace(tag))
            return View(viewModel);

        var isRecruiterOrAdmin = User.IsInRole("Recruiter") || User.IsInRole("Administrator");

        IQueryable<Position> positionsQuery = _context.Positions
            .Include(p => p.RelevantProjectTags);

        if (!isRecruiterOrAdmin)
            positionsQuery = positionsQuery.Where(p => p.IsPublic);

        if (!string.IsNullOrWhiteSpace(tag))
        {
            positionsQuery = positionsQuery.Where(p => p.RelevantProjectTags.Any(t => t.Name == tag));
        }
        else
        {
            positionsQuery = positionsQuery.Where(p =>
                EF.Functions.ToTsVector("english", p.Title + " " + p.ShortDescription)
                    .Matches(EF.Functions.PlainToTsQuery("english", term!)));
        }

        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Candidate") && !isRecruiterOrAdmin)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.ProfileId != null)
            {
                var profile = await _context.Profiles
                    .Include(p => p.AttributeValues)
                        .ThenInclude(v => v.SelectedOption)
                    .FirstOrDefaultAsync(p => p.Id == user.ProfileId.Value);

                if (profile is not null)
                {
                    var candidatePositions = await positionsQuery
                        .Include(p => p.AccessRules)
                        .ToListAsync();
                    candidatePositions = candidatePositions
                        .Where(p => PositionAccessEvaluator.IsAllowed(profile, p))
                        .ToList();

                    viewModel.PositionResults = candidatePositions
                        .Select(p => new PositionSearchItem { Id = p.Id, Title = p.Title, ShortDescription = p.ShortDescription })
                        .Take(20)
                        .ToList();
                }
            }
        }
        else
        {
            viewModel.PositionResults = await positionsQuery
                .Select(p => new PositionSearchItem { Id = p.Id, Title = p.Title, ShortDescription = p.ShortDescription })
                .Take(20)
                .ToListAsync();
        }

        if (isRecruiterOrAdmin)
        {
            IQueryable<Cv> cvsQuery = _context.Cvs
                .Where(c => c.Status == CvStatus.Published)
                .Include(c => c.Profile)
                .Include(c => c.Position);

            if (!string.IsNullOrWhiteSpace(tag))
            {
                cvsQuery = cvsQuery.Where(c => c.Profile.Projects.Any(pr => pr.Tags.Any(t => t.Name == tag)));
            }
            else
            {
                cvsQuery = cvsQuery.Where(c =>
                    EF.Functions.ToTsVector("english", c.Profile.FirstName + " " + c.Profile.LastName + " " + c.Position.Title)
                        .Matches(EF.Functions.PlainToTsQuery("english", term!)));
            }

            viewModel.CvResults = await cvsQuery
                .Select(c => new CvSearchItem
                {
                    Id = c.Id,
                    CandidateName = c.Profile.FirstName + " " + c.Profile.LastName,
                    PositionTitle = c.Position.Title,
                    LikesCount = _context.Set<CvLike>().Count(l => l.CvId == c.Id)
                })
                .Take(20)
                .ToListAsync();
        }

        return View(viewModel);
    }
}
