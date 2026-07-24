namespace CvManagement.Web.Models.Profile;

public class ProfileEditViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public uint Version { get; set; }
    public List<ProfileAttributeFieldViewModel> InfoAttributes { get; set; } = new();
}

public class ProfileAttributeFieldViewModel
{
    public Guid AttributeDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? StringValue { get; set; }
    public string? TextValue { get; set; }
    public decimal? NumericValue { get; set; }
    public DateOnly? DateValue { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public bool? BooleanValue { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public List<AttributeOptionChoice> Options { get; set; } = new();
}

public class AttributeOptionChoice
{
    public Guid Id { get; set; }
    public string Value { get; set; } = string.Empty;
}
