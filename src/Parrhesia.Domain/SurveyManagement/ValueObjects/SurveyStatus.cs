using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class SurveyStatus : ValueObject
{
    public static readonly SurveyStatus Draft = new("Draft");
    public static readonly SurveyStatus Active = new("Active");
    public static readonly SurveyStatus Closed = new("Closed");

    public string Value { get; }

#pragma warning disable CS8618
    private SurveyStatus() { }
#pragma warning restore CS8618

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

    public static SurveyStatus Reconstitute(string value) => new(value);

    public bool IsDraft => Value == "Draft";
    public bool IsActive => Value == "Active";
    public bool IsClosed => Value == "Closed";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}