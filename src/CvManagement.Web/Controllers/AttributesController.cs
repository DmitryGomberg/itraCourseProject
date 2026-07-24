using CvManagement.Domain;
using CvManagement.Domain.Entities;
using CvManagement.Infrastructure.Data;
using CvManagement.Web.Models.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize(Roles = "Recruiter,Administrator")]
public class AttributesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AttributesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? prefix, AttributeCategory? category, bool recentOnly = false)
    {
        IQueryable<AttributeDefinition> query = _context.AttributeDefinitions
            .Include(a => a.Options);

        if (!string.IsNullOrWhiteSpace(prefix))
            query = query.Where(a => EF.Functions.ILike(a.Name, prefix + "%"));

        if (category.HasValue)
            query = query.Where(a => a.Category == category.Value);

        if (recentOnly)
            query = query.Where(a => a.LastUsedAt != null).OrderByDescending(a => a.LastUsedAt);
        else
            query = query.OrderBy(a => a.Name);

        var items = await query.Select(a => new AttributeListItemViewModel
        {
            Id = a.Id,
            Name = a.Name,
            Category = a.Category.ToString(),
            DataTypeDisplay = a.DataType.ToString(),
            Description = a.Description,
            OptionsCount = a.Options.Count
        }).ToListAsync();

        var viewModel = new AttributeIndexViewModel
        {
            Items = items,
            Prefix = prefix,
            SelectedCategory = category,
            RecentOnly = recentOnly,
            Categories = new SelectList(Enum.GetValues<AttributeCategory>())
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new AttributeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttributeFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Form", model);

        if (await _context.AttributeDefinitions.AnyAsync(a => a.Name == model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Атрибут с таким именем уже существует");
            return View("Form", model);
        }

        var entity = new AttributeDefinition
        {
            Name = model.Name,
            Description = model.Description,
            Category = model.Category,
            DataType = model.DataType
        };

        _context.AttributeDefinitions.Add(entity);

        if (model.DataType == AttributeDataType.Option)
        {
            foreach (var line in model.Options.Where(o => !string.IsNullOrWhiteSpace(o)))
            {
                entity.Options.Add(new AttributeOption { Value = line.Trim() });
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _context.AttributeDefinitions
            .Include(a => a.Options)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity is null)
            return NotFound();

        var model = new AttributeFormViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Category = entity.Category,
            DataType = entity.DataType,
            Options = entity.Options.Select(o => o.Value).ToList(),
            Version = entity.Version
        };

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AttributeFormViewModel model)
    {
        var entity = await _context.AttributeDefinitions
            .Include(a => a.Options)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity is null)
            return NotFound();

        if (model.Version.HasValue)
        {
            _context.Entry(entity).Property("Version").OriginalValue = model.Version.Value;
        }

        if (!ModelState.IsValid)
        {
            model.Version = entity.Version;
            return View("Form", model);
        }

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Category = model.Category;
        entity.DataType = model.DataType;

        if (model.DataType == AttributeDataType.Option)
        {
            var existingValues = entity.Options.Select(o => o.Value).ToList();
            var newValues = model.Options.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()).ToList();

            var toRemove = entity.Options.Where(o => !newValues.Contains(o.Value)).ToList();
            _context.AttributeOptions.RemoveRange(toRemove);

            var toAdd = newValues.Where(v => !existingValues.Contains(v)).ToList();
            foreach (var val in toAdd)
            {
                var newOption = new AttributeOption
                {
                    AttributeDefinitionId = entity.Id,
                    Value = val
                };
                entity.Options.Add(newOption);
                _context.Entry(newOption).State = EntityState.Added;
            }
        }
        else
        {
            _context.AttributeOptions.RemoveRange(entity.Options);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var stillExists = await _context.AttributeDefinitions.AnyAsync(a => a.Id == id);
            if (!stillExists)
            {
                TempData["Error"] = "Этот атрибут был удалён другим пользователем";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty,
                "Запись была изменена другим пользователем, обновите страницу");

            var fresh = await _context.AttributeDefinitions
                .Include(a => a.Options)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (fresh is not null)
            {
                model.Name = fresh.Name;
                model.Description = fresh.Description;
                model.Category = fresh.Category;
                model.DataType = fresh.DataType;
                model.Options = fresh.Options.Select(o => o.Value).ToList();
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
        var entities = await _context.AttributeDefinitions
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();

        _context.AttributeDefinitions.RemoveRange(entities);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Нельзя удалить атрибут, который используется";
        }

        return RedirectToAction(nameof(Index));
    }
}
