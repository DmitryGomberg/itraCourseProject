using CvManagement.Domain;
using CvManagement.Domain.Entities;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
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
            .FirstOrDefaultAsync(p => p.Id == user.ProfileId.Value);

        if (profile is null)
            return BadRequest("Profile not found");

        var allAttributes = await _context.AttributeDefinitions
            .Include(a => a.Options)
            .OrderBy(a => a.Name)
            .ToListAsync();

        var avDict = profile.AttributeValues
            .ToDictionary(av => av.AttributeDefinitionId);

        var infoFields = allAttributes.Select(def =>
        {
            var av = avDict.TryGetValue(def.Id, out var val) ? val : null;
            var field = new ProfileAttributeFieldViewModel
            {
                AttributeDefinitionId = def.Id,
                Name = def.Name,
                DataType = def.DataType.ToString(),
                StringValue = av?.StringValue,
                TextValue = av?.TextValue,
                NumericValue = av?.NumericValue,
                DateValue = av?.DateValue,
                PeriodStart = av?.PeriodStart,
                PeriodEnd = av?.PeriodEnd,
                BooleanValue = av?.BooleanValue,
                SelectedOptionId = av?.SelectedOptionId,
                Options = def.Options.Select(o => new AttributeOptionChoice
                {
                    Id = o.Id,
                    Value = o.Value
                }).ToList()
            };
            return field;
        }).ToList();

        var viewModel = new ProfileEditViewModel
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Location = profile.Location,
            PhotoUrl = profile.PhotoUrl,
            Version = profile.Version,
            InfoAttributes = infoFields
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateField([FromBody] UpdateProfileFieldRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.ProfileId is null)
            return NotFound();

        var profile = await _context.Profiles.FindAsync(user.ProfileId.Value);
        if (profile is null)
            return NotFound();

        _context.Entry(profile).Property("Version").OriginalValue = request.Version;

        switch (request.Field)
        {
            case "FirstName":
                profile.FirstName = request.Value;
                break;
            case "LastName":
                profile.LastName = request.Value;
                break;
            case "Location":
                profile.Location = request.Value;
                break;
            case "PhotoUrl":
                profile.PhotoUrl = request.Value;
                break;
            default:
                return BadRequest(new { message = "Unknown field" });
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "Профиль был изменён в другом месте, обновите страницу" });
        }

        var newVersion = _context.Entry(profile).Property("Version").CurrentValue;
        return Ok(new { newVersion });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAttributeValue([FromBody] UpdateAttributeValueRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.ProfileId is null)
            return NotFound();

        var existing = await _context.AttributeValues
            .FirstOrDefaultAsync(av =>
                av.ProfileId == user.ProfileId.Value &&
                av.AttributeDefinitionId == request.AttributeDefinitionId);

        if (existing is null)
        {
            var newValue = new AttributeValue
            {
                ProfileId = user.ProfileId.Value,
                AttributeDefinitionId = request.AttributeDefinitionId,
                StringValue = request.StringValue,
                TextValue = request.TextValue,
                NumericValue = request.NumericValue,
                DateValue = request.DateValue,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                BooleanValue = request.BooleanValue,
                SelectedOptionId = request.SelectedOptionId
            };
            _context.AttributeValues.Add(newValue);
            _context.Entry(newValue).State = EntityState.Added;
        }
        else
        {
            if (request.StringValue is not null) existing.StringValue = request.StringValue;
            if (request.TextValue is not null) existing.TextValue = request.TextValue;
            if (request.NumericValue.HasValue) existing.NumericValue = request.NumericValue;
            if (request.DateValue.HasValue) existing.DateValue = request.DateValue;
            if (request.PeriodStart.HasValue) existing.PeriodStart = request.PeriodStart;
            if (request.PeriodEnd.HasValue) existing.PeriodEnd = request.PeriodEnd;
            if (request.BooleanValue.HasValue) existing.BooleanValue = request.BooleanValue;
            if (request.SelectedOptionId.HasValue) existing.SelectedOptionId = request.SelectedOptionId;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}
