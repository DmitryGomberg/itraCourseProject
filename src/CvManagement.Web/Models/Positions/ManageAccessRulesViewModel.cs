using System.ComponentModel.DataAnnotations;
using CvManagement.Domain;

namespace CvManagement.Web.Models.Positions;

public class ManageAccessRulesViewModel
{
    public Guid PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public List<AccessRuleRow> Rules { get; set; } = new();
    public List<AttributeOptionItem> AllAttributes { get; set; } = new();
    public Dictionary<string, string[]> OperatorAllowedTypes { get; set; } = new();
}

public class AccessRuleRow
{
    public Guid Id { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string OperatorDisplay { get; set; } = string.Empty;
    public string ComparisonValue { get; set; } = string.Empty;
}

public class AttributeOptionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
}
