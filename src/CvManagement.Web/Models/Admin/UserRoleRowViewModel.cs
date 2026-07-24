namespace CvManagement.Web.Models.Admin;

public class UserRoleRowViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> AllRoles { get; set; } = new();
    public List<string> AssignedRoles { get; set; } = new();
}
