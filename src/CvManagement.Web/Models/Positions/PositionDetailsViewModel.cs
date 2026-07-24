namespace CvManagement.Web.Models.Positions;

public class PositionDetailsViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int MaxProjects { get; set; }
    public List<PositionAttributeSummary> Attributes { get; set; } = new();
    public List<PositionAccessRuleSummary> AccessRules { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<PositionCvSummary> Cvs { get; set; } = new();
}

public class PositionCvSummary
{
    public Guid Id { get; set; }
    public string CandidateFirstLastName { get; set; } = string.Empty;
    public int LikesCount { get; set; }
}

public class PositionAttributeSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataTypeDisplay { get; set; } = string.Empty;
}

public class PositionAccessRuleSummary
{
    public Guid Id { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string OperatorDisplay { get; set; } = string.Empty;
    public string ComparisonValue { get; set; } = string.Empty;
}
