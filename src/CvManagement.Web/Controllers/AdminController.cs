using CvManagement.Infrastructure.Identity;
using CvManagement.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvManagement.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var allRoleNames = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
        var allUsers = await _userManager.Users.ToListAsync();

        var rows = new List<UserRoleRowViewModel>();
        foreach (var user in allUsers)
        {
            var assignedRoles = await _userManager.GetRolesAsync(user);
            rows.Add(new UserRoleRowViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                AllRoles = allRoleNames,
                AssignedRoles = assignedRoles.ToList()
            });
        }

        return View(rows);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRoles(UpdateUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var newRoles = request.Roles ?? Array.Empty<string>();

        await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(newRoles).ToArray());
        await _userManager.AddToRolesAsync(user, newRoles.Except(currentRoles).ToArray());

        return RedirectToAction(nameof(Users));
    }
}
