using CvManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers.Api;

[ApiController]
[Route("api/tags")]
[Authorize]
public class TechnologyTagsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TechnologyTagsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q)
    {
        var query = _context.TechnologyTags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => EF.Functions.ILike(t.Name, "%" + q + "%"));

        var tags = await query
            .OrderBy(t => t.Name)
            .Select(t => new { id = t.Id, name = t.Name })
            .Take(20)
            .ToListAsync();

        return Ok(tags);
    }
}
