using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class OptionText : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

#pragma warning disable CS8618
    private OptionText() { }
#pragma warning restore CS8618

    private OptionText(string value)
    {
        Value = value;
    }

    public static OptionText Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Option text cannot be empty", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Option text cannot exceed {MaxLength} characters", nameof(value));

        return new OptionText(trimmed);
    }

    public static OptionText Reconstitute(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(OptionText text) => text.Value;
}