using System.ComponentModel.DataAnnotations;
using CvManagement.Domain;

namespace CvManagement.Web.Models.Attributes;

public class AttributeFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public AttributeCategory Category { get; set; }

    [Required]
    public AttributeDataType DataType { get; set; }

    public List<string> Options { get; set; } = new();

    public uint? Version { get; set; }
}
