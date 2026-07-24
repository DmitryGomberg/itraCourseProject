using Microsoft.AspNetCore.Identity;

namespace CvManagement.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid? ProfileId { get; set; }
}
