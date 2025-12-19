using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class QuestionId : ValueObject
{
    public Guid Value { get; }

    private QuestionId() { }

    private QuestionId(Guid value)
    {
        Value = value;
    }

    public static QuestionId NewId() => new(Guid.NewGuid());

    public static QuestionId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("QuestionId cannot be empty", nameof(value));

        return new QuestionId(value);
    }

    public static QuestionId Reconstitute(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(QuestionId id) => id.Value;
}