using System.Diagnostics;
using CvManagement.Domain;
using CvManagement.Domain.Entities;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Models;
using CvManagement.Web.Models.Home;
using CvManagement.Web.Models.Positions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var candidateUsers = await _userManager.GetUsersInRoleAsync("Candidate");

        var totalPositions = await _context.Positions.CountAsync();
        var totalPublishedCvs = await _context.Cvs.CountAsync(c => c.Status == CvStatus.Published);

        var latestPositions = await _context.Positions
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new PositionListItemViewModel
            {
                Id = p.Id,
                Title = p.Title,
                ShortDescriptionShort = p.ShortDescription.Length > 100
                    ? p.ShortDescription.Substring(0, 100) + "..."
                    : p.ShortDescription,
                IsPublic = p.IsPublic,
                MaxProjects = p.MaxProjects,
                AttributesCount = p.PositionAttributes.Count,
                AccessRulesCount = p.AccessRules.Count
            })
            .ToListAsync();

        var topPositions = await _context.Positions
            .Select(p => new TopPositionItem
            {
                Id = p.Id,
                Title = p.Title,
                CvCount = p.Cvs.Count(c => c.Status == CvStatus.Published)
            })
            .OrderByDescending(x => x.CvCount)
            .Take(5)
            .ToListAsync();

        var tagCloud = await _context.TechnologyTags
            .Select(t => new TagCloudItem
            {
                Name = t.Name,
                UsageCount = t.Projects.Count
            })
            .Where(x => x.UsageCount > 0)
            .OrderByDescending(x => x.UsageCount)
            .Take(20)
            .ToListAsync();

        var viewModel = new HomeIndexViewModel
        {
            TotalCandidates = candidateUsers.Count,
            TotalPositions = totalPositions,
            TotalPublishedCvs = totalPublishedCvs,
            LatestPositions = latestPositions,
            TopPositions = topPositions,
            TagCloud = tagCloud
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
