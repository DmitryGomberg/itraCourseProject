namespace CvManagement.Web.Models.Admin;

public class UpdateUserRolesRequest
{
    public string UserId { get; set; } = string.Empty;
    public string[]? Roles { get; set; }
}
