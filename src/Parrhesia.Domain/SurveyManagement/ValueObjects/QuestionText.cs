using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class QuestionText : ValueObject
{
    public const int MaxLength = 500;

    public string Value { get; }

#pragma warning disable CS8618
    private QuestionText() { }
#pragma warning restore CS8618

    private QuestionText(string value)
    {
        Value = value;
    }

    public static QuestionText Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Question text cannot be empty", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"Question text cannot exceed {MaxLength} characters", nameof(value));

        return new QuestionText(trimmed);
    }

    public static QuestionText Reconstitute(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(QuestionText text) => text.Value;
}