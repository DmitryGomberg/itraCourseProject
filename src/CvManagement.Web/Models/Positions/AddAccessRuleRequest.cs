using System.ComponentModel.DataAnnotations;
using CvManagement.Domain;

namespace CvManagement.Web.Models.Positions;

public class AddAccessRuleRequest
{
    [Required]
    public Guid AttributeDefinitionId { get; set; }

    [Required]
    public AccessRuleOperator Operator { get; set; }

    [Required]
    public string ComparisonValue { get; set; } = string.Empty;
}
