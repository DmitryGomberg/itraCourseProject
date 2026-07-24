using CvManagement.Domain;

namespace CvManagement.Domain.Services;

public static class AccessRuleOperatorRules
{
    public static readonly IReadOnlyDictionary<AttributeDataType, AccessRuleOperator[]> AllowedOperators =
        new Dictionary<AttributeDataType, AccessRuleOperator[]>
        {
            [AttributeDataType.String] = new[] { AccessRuleOperator.Equals, AccessRuleOperator.NotEquals },
            [AttributeDataType.Text] = new[] { AccessRuleOperator.Equals, AccessRuleOperator.NotEquals },
            [AttributeDataType.Option] = new[] { AccessRuleOperator.Equals, AccessRuleOperator.NotEquals },
            [AttributeDataType.Numeric] = new[] { AccessRuleOperator.Equals, AccessRuleOperator.NotEquals, AccessRuleOperator.GreaterThan, AccessRuleOperator.GreaterOrEqual, AccessRuleOperator.LessThan, AccessRuleOperator.LessOrEqual },
            [AttributeDataType.Date] = new[] { AccessRuleOperator.Equals, AccessRuleOperator.NotEquals, AccessRuleOperator.GreaterThan, AccessRuleOperator.GreaterOrEqual, AccessRuleOperator.LessThan, AccessRuleOperator.LessOrEqual },
            [AttributeDataType.Period] = new[] { AccessRuleOperator.Equals, AccessRuleOperator.NotEquals, AccessRuleOperator.GreaterThan, AccessRuleOperator.GreaterOrEqual, AccessRuleOperator.LessThan, AccessRuleOperator.LessOrEqual },
            [AttributeDataType.Boolean] = new[] { AccessRuleOperator.IsTrue, AccessRuleOperator.IsFalse },
            [AttributeDataType.Image] = Array.Empty<AccessRuleOperator>()
        };

    public static bool IsAllowed(AttributeDataType dataType, AccessRuleOperator op) =>
        AllowedOperators.TryGetValue(dataType, out var ops) && ops.Contains(op);
}
