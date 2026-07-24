using CvManagement.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CvManagement.Web.Models.Attributes;

public class AttributeIndexViewModel
{
    public List<AttributeListItemViewModel> Items { get; set; } = new();
    public string? Prefix { get; set; }
    public AttributeCategory? SelectedCategory { get; set; }
    public bool RecentOnly { get; set; }
    public SelectList? Categories { get; set; }
}
