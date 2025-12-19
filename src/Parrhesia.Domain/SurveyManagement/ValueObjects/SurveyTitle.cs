using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class SurveyTitle : ValueObject
{
    public const int MaxLength = 500;

    public string Value { get; }

#pragma warning disable CS8618
    private SurveyTitle() { }
#pragma warning restore CS8618

    private SurveyTitle(string value)
    {
        Value = value;
    }

    public static SurveyTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Survey title cannot be empty", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Survey title cannot exceed {MaxLength} characters", nameof(value));

        return new SurveyTitle(trimmed);
    }

    public static SurveyTitle Reconstitute(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(SurveyTitle title) => title.Value;
}