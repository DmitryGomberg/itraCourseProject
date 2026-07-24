using CvManagement.Domain.Entities;

namespace CvManagement.Domain.Services;

public static class PositionAccessEvaluator
{
    public static bool IsAllowed(Profile profile, Position position)
    {
        foreach (var rule in position.AccessRules)
        {
            var value = profile.AttributeValues
                .FirstOrDefault(v => v.AttributeDefinitionId == rule.AttributeDefinitionId);

            if (!EvaluateRule(rule, value))
                return false;
        }
        return true;
    }

    private static bool EvaluateRule(PositionAccessRule rule, AttributeValue? value)
    {
        if (value is null)
            return false;

        switch (rule.Operator)
        {
            case AccessRuleOperator.IsTrue:
                return value.BooleanValue == true;
            case AccessRuleOperator.IsFalse:
                return value.BooleanValue == false;
        }

        if (value.StringValue is not null || value.TextValue is not null || value.SelectedOptionId is not null)
        {
            string? actualText = value.StringValue ?? value.TextValue
                ?? value.SelectedOption?.Value;

            bool textEquals = string.Equals(actualText?.Trim(), rule.ComparisonValue?.Trim(), StringComparison.OrdinalIgnoreCase);
            return rule.Operator == AccessRuleOperator.Equals ? textEquals : !textEquals;
        }

        if (value.NumericValue.HasValue && decimal.TryParse(rule.ComparisonValue, out var numComparison))
        {
            return Compare(value.NumericValue.Value.CompareTo(numComparison), rule.Operator);
        }

        if (value.DateValue.HasValue && DateOnly.TryParse(rule.ComparisonValue, out var dateComparison))
        {
            return Compare(value.DateValue.Value.CompareTo(dateComparison), rule.Operator);
        }

        return false;
    }

    private static bool Compare(int comparisonResult, AccessRuleOperator op) => op switch
    {
        AccessRuleOperator.Equals => comparisonResult == 0,
        AccessRuleOperator.NotEquals => comparisonResult != 0,
        AccessRuleOperator.GreaterThan => comparisonResult > 0,
        AccessRuleOperator.GreaterOrEqual => comparisonResult >= 0,
        AccessRuleOperator.LessThan => comparisonResult < 0,
        AccessRuleOperator.LessOrEqual => comparisonResult <= 0,
        _ => false
    };
}
