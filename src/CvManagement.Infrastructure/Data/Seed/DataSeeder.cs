using CvManagement.Domain;
using CvManagement.Domain.Entities;
using CvManagement.Infrastructure.Data;
using CvManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CvManagement.Infrastructure.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        string[] roles = ["Candidate", "Recruiter", "Administrator"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@cvmanagement.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var profile = new Profile
            {
                UserId = adminUser.Id,
                FirstName = "Admin",
                LastName = "Admin",
                Location = "N/A"
            };

            dbContext.Profiles.Add(profile);
            await dbContext.SaveChangesAsync();

            adminUser.ProfileId = profile.Id;
            await userManager.UpdateAsync(adminUser);

            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }

        if (!await dbContext.AttributeDefinitions.AnyAsync())
        {
            var englishLevel = new AttributeDefinition
            {
                Category = AttributeCategory.Language,
                Name = "English Level",
                Description = "CEFR level",
                DataType = AttributeDataType.Option,
                Options =
                [
                    new AttributeOption { Value = "A1" },
                    new AttributeOption { Value = "A2" },
                    new AttributeOption { Value = "B1" },
                    new AttributeOption { Value = "B2" },
                    new AttributeOption { Value = "C1" },
                    new AttributeOption { Value = "C2" }
                ]
            };

            var gpa = new AttributeDefinition
            {
                Category = AttributeCategory.DomainKnowledge,
                Name = "GPA",
                Description = "Grade point average",
                DataType = AttributeDataType.Numeric
            };

            var ielts = new AttributeDefinition
            {
                Category = AttributeCategory.Language,
                Name = "IELTS Score",
                Description = "IELTS band score",
                DataType = AttributeDataType.Numeric
            };

            var remoteWork = new AttributeDefinition
            {
                Category = AttributeCategory.SoftSkills,
                Name = "Remote Work Availability",
                Description = "Available for remote work",
                DataType = AttributeDataType.Boolean
            };

            var presentation = new AttributeDefinition
            {
                Category = AttributeCategory.SoftSkills,
                Name = "Presentation Skills",
                Description = "Presentation skill level",
                DataType = AttributeDataType.Option,
                Options =
                [
                    new AttributeOption { Value = "Basic" },
                    new AttributeOption { Value = "Intermediate" },
                    new AttributeOption { Value = "Advanced" }
                ]
            };

            dbContext.AttributeDefinitions.AddRange(englishLevel, gpa, ielts, remoteWork, presentation);
            await dbContext.SaveChangesAsync();
        }
    }
}
