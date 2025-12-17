using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class SurveyStatus : ValueObject
{
    public static readonly SurveyStatus Draft = new("Draft");
    public static readonly SurveyStatus Active = new("Active");
    public static readonly SurveyStatus Closed = new("Closed");

    public string Value { get; }

    private SurveyStatus(string value)
    {
        Value = value;
    }

    public static SurveyStatus FromString(string value)
    {
        return value switch
        {
            "Draft" => Draft,
            "Active" => Active,
            "Closed" => Closed,
            _ => throw new ArgumentException($"Invalid survey status: {value}", nameof(value))
        };
    }

    public bool IsDraft => this == Draft;
    public bool IsActive => this == Active;
    public bool IsClosed => this == Closed;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
