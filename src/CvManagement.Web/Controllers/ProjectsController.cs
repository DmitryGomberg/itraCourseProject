using CvManagement.Domain.Entities;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Models.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<Guid?> GetCurrentProfileId()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.ProfileId;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var items = await _context.Projects
            .Where(p => p.ProfileId == profileId.Value)
            .Include(p => p.Tags)
            .OrderByDescending(p => p.PeriodStart)
            .Select(p => new ProjectListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                PeriodDisplay = $"{p.PeriodStart:MMM yyyy} - {(p.PeriodEnd.HasValue ? p.PeriodEnd.Value.ToString("MMM yyyy") : "Present")}",
                TagsDisplay = string.Join(", ", p.Tags.Select(t => t.Name)),
                DescriptionShort = p.DescriptionMarkdown.Length > 100
                    ? p.DescriptionMarkdown.Substring(0, 100) + "..."
                    : p.DescriptionMarkdown
            })
            .ToListAsync();

        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new ProjectFormViewModel
        {
            PeriodStart = DateOnly.FromDateTime(DateTime.Today)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectFormViewModel model)
    {
        if (!model.IsOngoing && model.PeriodEnd is null)
            ModelState.AddModelError(nameof(model.PeriodEnd), "Укажите дату окончания или отметьте текущий проект");

        if (!ModelState.IsValid)
            return View("Form", model);

        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var entity = new Project
        {
            ProfileId = profileId.Value,
            Name = model.Name,
            PeriodStart = model.PeriodStart,
            PeriodEnd = model.IsOngoing ? null : model.PeriodEnd,
            DescriptionMarkdown = model.DescriptionMarkdown
        };

        await SyncTagsAsync(entity, model.TagsInput);

        _context.Projects.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var entity = await _context.Projects
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id && p.ProfileId == profileId.Value);

        if (entity is null)
            return NotFound();

        var model = new ProjectFormViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            PeriodStart = entity.PeriodStart,
            IsOngoing = entity.PeriodEnd is null,
            PeriodEnd = entity.PeriodEnd,
            DescriptionMarkdown = entity.DescriptionMarkdown,
            TagsInput = string.Join(", ", entity.Tags.Select(t => t.Name)),
            Version = entity.Version
        };

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProjectFormViewModel model)
    {
        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var entity = await _context.Projects
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id && p.ProfileId == profileId.Value);

        if (entity is null)
            return NotFound();

        if (model.Version.HasValue)
            _context.Entry(entity).Property("Version").OriginalValue = model.Version.Value;

        if (!model.IsOngoing && model.PeriodEnd is null)
            ModelState.AddModelError(nameof(model.PeriodEnd), "Укажите дату окончания или отметьте текущий проект");

        if (!ModelState.IsValid)
        {
            model.Version = entity.Version;
            return View("Form", model);
        }

        entity.Name = model.Name;
        entity.PeriodStart = model.PeriodStart;
        entity.PeriodEnd = model.IsOngoing ? null : model.PeriodEnd;
        entity.DescriptionMarkdown = model.DescriptionMarkdown;

        entity.Tags.Clear();
        await SyncTagsAsync(entity, model.TagsInput);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var stillExists = await _context.Projects.AnyAsync(p => p.Id == id);
            if (!stillExists)
            {
                TempData["Error"] = "Этот проект был удалён другим пользователем";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty,
                "Запись была изменена другим пользователем, обновите страницу");

            var fresh = await _context.Projects.FindAsync(id);
            if (fresh is not null)
            {
                model.Name = fresh.Name;
                model.PeriodStart = fresh.PeriodStart;
                model.IsOngoing = fresh.PeriodEnd is null;
                model.PeriodEnd = fresh.PeriodEnd;
                model.DescriptionMarkdown = fresh.DescriptionMarkdown;
                model.Version = fresh.Version;
            }

            return View("Form", model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid[] ids)
    {
        var profileId = await GetCurrentProfileId();
        if (profileId is null)
            return BadRequest("Profile not found");

        var entities = await _context.Projects
            .Where(p => p.ProfileId == profileId.Value && ids.Contains(p.Id))
            .ToListAsync();

        _context.Projects.RemoveRange(entities);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Нельзя удалить проект, который используется";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SyncTagsAsync(Project entity, string? tagsInput)
    {
        if (string.IsNullOrWhiteSpace(tagsInput))
            return;

        var tagNames = tagsInput
            .Split(',')
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var tagName in tagNames)
        {
            var tag = await _context.TechnologyTags
                .FirstOrDefaultAsync(t => t.Name == tagName);

            if (tag is null)
            {
                tag = new TechnologyTag { Name = tagName };
                _context.TechnologyTags.Add(tag);
                _context.Entry(tag).State = EntityState.Added;
            }

            entity.Tags.Add(tag);
        }
    }
}
