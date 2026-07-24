using CvManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize]
public class PublicProfileController : Controller
{
    private readonly ApplicationDbContext _context;

    public PublicProfileController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("PublicProfile/{profileId:guid}")]
    public async Task<IActionResult> Index(Guid profileId)
    {
        var profile = await _context.Profiles.FindAsync(profileId);
        if (profile is null)
            return NotFound();

        return View(profile);
    }
}
