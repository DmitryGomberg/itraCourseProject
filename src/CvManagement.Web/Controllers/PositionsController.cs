using CvManagement.Domain;
using CvManagement.Domain.Entities;
using CvManagement.Domain.Services;
using CvManagement.Infrastructure.Data;
using CvManagement.Web.Models.Positions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize(Roles = "Recruiter,Administrator")]
public class PositionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PositionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        IQueryable<Position> query = _context.Positions;

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.ILike(p.Title, "%" + search + "%"));

        var items = await query
            .OrderBy(p => p.Title)
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

        var viewModel = new PositionIndexViewModel
        {
            Items = items,
            Search = search
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Form", new PositionFormViewModel { MaxProjects = 5 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PositionFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Form", model);

        var entity = new Position
        {
            Title = model.Title,
            ShortDescription = model.ShortDescription,
            IsPublic = model.IsPublic,
            MaxProjects = model.MaxProjects
        };

        _context.Positions.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = entity.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _context.Positions.FindAsync(id);

        if (entity is null)
            return NotFound();

        var model = new PositionFormViewModel
        {
            Id = entity.Id,
            Title = entity.Title,
            ShortDescription = entity.ShortDescription,
            IsPublic = entity.IsPublic,
            MaxProjects = entity.MaxProjects,
            Version = entity.Version
        };

        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PositionFormViewModel model)
    {
        var entity = await _context.Positions.FindAsync(id);

        if (entity is null)
            return NotFound();

        if (model.Version.HasValue)
            _context.Entry(entity).Property("Version").OriginalValue = model.Version.Value;

        if (!ModelState.IsValid)
        {
            model.Version = entity.Version;
            return View("Form", model);
        }

        entity.Title = model.Title;
        entity.ShortDescription = model.ShortDescription;
        entity.IsPublic = model.IsPublic;
        entity.MaxProjects = model.MaxProjects;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var stillExists = await _context.Positions.AnyAsync(p => p.Id == id);
            if (!stillExists)
            {
                TempData["Error"] = "Эта позиция была удалена другим пользователем";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty,
                "Запись была изменена другим пользователем, обновите страницу");

            var fresh = await _context.Positions.FindAsync(id);
            if (fresh is not null)
            {
                model.Title = fresh.Title;
                model.ShortDescription = fresh.ShortDescription;
                model.IsPublic = fresh.IsPublic;
                model.MaxProjects = fresh.MaxProjects;
                model.Version = fresh.Version;
            }

            return View("Form", model);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var entity = await _context.Positions
            .Include(p => p.PositionAttributes).ThenInclude(pa => pa.AttributeDefinition)
            .Include(p => p.AccessRules).ThenInclude(r => r.AttributeDefinition)
            .Include(p => p.RelevantProjectTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity is null)
            return NotFound();

        var viewModel = new PositionDetailsViewModel
        {
            Id = entity.Id,
            Title = entity.Title,
            ShortDescription = entity.ShortDescription,
            IsPublic = entity.IsPublic,
            MaxProjects = entity.MaxProjects,
            Attributes = entity.PositionAttributes
                .OrderBy(pa => pa.DisplayOrder)
                .Select(pa => new PositionAttributeSummary
                {
                    Id = pa.AttributeDefinition.Id,
                    Name = pa.AttributeDefinition.Name,
                    DataTypeDisplay = pa.AttributeDefinition.DataType.ToString()
                })
                .ToList(),
            AccessRules = entity.AccessRules
                .Select(r => new PositionAccessRuleSummary
                {
                    Id = r.Id,
                    AttributeName = r.AttributeDefinition.Name,
                    OperatorDisplay = r.Operator.ToString(),
                    ComparisonValue = r.ComparisonValue
                })
                .ToList(),
            Tags = entity.RelevantProjectTags.Select(t => t.Name).ToList()
        };

        var cvs = await _context.Cvs
            .Where(c => c.PositionId == id && c.Status == CvStatus.Published)
            .Include(c => c.Profile)
            .Select(c => new PositionCvSummary
            {
                Id = c.Id,
                CandidateFirstLastName = c.Profile.FirstName + " " + c.Profile.LastName,
                LikesCount = _context.Set<CvLike>().Count(l => l.CvId == c.Id)
            })
            .ToListAsync();

        viewModel.Cvs = cvs.OrderByDescending(c => c.LikesCount).ToList();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid[] ids)
    {
        var entities = await _context.Positions
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        _context.Positions.RemoveRange(entities);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Нельзя удалить позицию, которая используется";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ManageAttributes(Guid id)
    {
        var position = await _context.Positions
            .Include(p => p.PositionAttributes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position is null)
            return NotFound();

        var allAttributes = await _context.AttributeDefinitions
            .OrderBy(a => a.Name)
            .ToListAsync();

        var selectedIds = position.PositionAttributes
            .Select(pa => pa.AttributeDefinitionId)
            .ToHashSet();

        var viewModel = new ManageAttributesViewModel
        {
            PositionId = position.Id,
            PositionTitle = position.Title,
            AllAttributes = allAttributes.Select(a => new AttributeCheckItem
            {
                Id = a.Id,
                Name = a.Name,
                Category = a.Category.ToString(),
                DataTypeDisplay = a.DataType.ToString(),
                IsSelected = selectedIds.Contains(a.Id)
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageAttributes(Guid id, Guid[] selectedAttributeIds)
    {
        var position = await _context.Positions
            .Include(p => p.PositionAttributes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position is null)
            return NotFound();

        var currentIds = position.PositionAttributes
            .Select(pa => pa.AttributeDefinitionId)
            .ToHashSet();

        var newIds = (selectedAttributeIds ?? Array.Empty<Guid>()).ToHashSet();

        var toRemove = position.PositionAttributes
            .Where(pa => !newIds.Contains(pa.AttributeDefinitionId))
            .ToList();

        _context.PositionAttributes.RemoveRange(toRemove);

        var toAddIds = newIds.Except(currentIds).ToList();

        if (toAddIds.Count > 0)
        {
            var currentMaxOrder = position.PositionAttributes.Any()
                ? position.PositionAttributes.Max(pa => pa.DisplayOrder)
                : 0;

            var definitionsToAdd = await _context.AttributeDefinitions
                .Where(a => toAddIds.Contains(a.Id))
                .ToListAsync();

            foreach (var def in definitionsToAdd)
            {
                currentMaxOrder++;
                var newPositionAttribute = new PositionAttribute
                {
                    PositionId = position.Id,
                    AttributeDefinitionId = def.Id,
                    DisplayOrder = currentMaxOrder
                };
                _context.PositionAttributes.Add(newPositionAttribute);
                _context.Entry(newPositionAttribute).State = EntityState.Added;

                def.LastUsedAt = DateTimeOffset.UtcNow;
            }
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Конфликт версий, попробуйте ещё раз";
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ManageAccessRules(Guid id)
    {
        var position = await _context.Positions
            .Include(p => p.AccessRules).ThenInclude(r => r.AttributeDefinition)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position is null)
            return NotFound();

        var allAttributes = await _context.AttributeDefinitions
            .OrderBy(a => a.Name)
            .ToListAsync();

        var operatorAllowedTypes = new Dictionary<string, string[]>();
        foreach (var kvp in AccessRuleOperatorRules.AllowedOperators)
        {
            foreach (var op in kvp.Value)
            {
                var opName = op.ToString();
                if (!operatorAllowedTypes.ContainsKey(opName))
                    operatorAllowedTypes[opName] = Array.Empty<string>();

                operatorAllowedTypes[opName] = operatorAllowedTypes[opName]
                    .Append(kvp.Key.ToString())
                    .ToArray();
            }
        }

        var viewModel = new ManageAccessRulesViewModel
        {
            PositionId = position.Id,
            PositionTitle = position.Title,
            Rules = position.AccessRules.Select(r => new AccessRuleRow
            {
                Id = r.Id,
                AttributeName = r.AttributeDefinition.Name,
                OperatorDisplay = r.Operator.ToString(),
                ComparisonValue = r.ComparisonValue
            }).ToList(),
            AllAttributes = allAttributes.Select(a => new AttributeOptionItem
            {
                Id = a.Id,
                Name = a.Name,
                DataType = a.DataType.ToString()
            }).ToList(),
            OperatorAllowedTypes = operatorAllowedTypes
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAccessRule(Guid id, AddAccessRuleRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Заполните все поля";
            return RedirectToAction(nameof(ManageAccessRules), new { id });
        }

        var attribute = await _context.AttributeDefinitions.FindAsync(request.AttributeDefinitionId);
        if (attribute is null)
            return NotFound();

        if (!AccessRuleOperatorRules.IsAllowed(attribute.DataType, request.Operator))
        {
            TempData["Error"] = $"Оператор недопустим для типа {attribute.DataType}";
            return RedirectToAction(nameof(ManageAccessRules), new { id });
        }

        var position = await _context.Positions.FindAsync(id);
        if (position is null)
            return NotFound();

        var entity = new PositionAccessRule
        {
            PositionId = id,
            AttributeDefinitionId = request.AttributeDefinitionId,
            Operator = request.Operator,
            ComparisonValue = request.ComparisonValue
        };

        _context.PositionAccessRules.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageAccessRules), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccessRules(Guid id, Guid[] ruleIds)
    {
        var entities = await _context.PositionAccessRules
            .Where(r => r.PositionId == id && ruleIds.Contains(r.Id))
            .ToListAsync();

        _context.PositionAccessRules.RemoveRange(entities);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageAccessRules), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ManageTags(Guid id)
    {
        var position = await _context.Positions
            .Include(p => p.RelevantProjectTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position is null)
            return NotFound();

        var viewModel = new ManageTagsViewModel
        {
            PositionId = position.Id,
            PositionTitle = position.Title,
            CurrentTags = position.RelevantProjectTags
                .OrderBy(t => t.Name)
                .Select(t => new TagItem { Id = t.Id, Name = t.Name })
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTags(Guid id, AddTagsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TagName))
        {
            TempData["Error"] = "Введите название тега";
            return RedirectToAction(nameof(ManageTags), new { id });
        }

        var position = await _context.Positions
            .Include(p => p.RelevantProjectTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position is null)
            return NotFound();

        var tagName = request.TagName.Trim();

        var tag = await _context.TechnologyTags
            .FirstOrDefaultAsync(t => t.Name == tagName);

        if (tag is null)
        {
            tag = new TechnologyTag { Name = tagName };
            _context.TechnologyTags.Add(tag);
        }

        if (!position.RelevantProjectTags.Any(t => t.Id == tag.Id))
            position.RelevantProjectTags.Add(tag);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageTags), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTags(Guid id, Guid[] tagIds)
    {
        var position = await _context.Positions
            .Include(p => p.RelevantProjectTags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (position is null)
            return NotFound();

        var toRemove = position.RelevantProjectTags
            .Where(t => tagIds.Contains(t.Id))
            .ToList();

        foreach (var tag in toRemove)
            position.RelevantProjectTags.Remove(tag);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(ManageTags), new { id });
    }
}
