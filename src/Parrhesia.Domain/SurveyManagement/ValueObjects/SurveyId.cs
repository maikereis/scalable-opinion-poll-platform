using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class SurveyId : ValueObject
{
    public Guid Value { get; }

    private SurveyId(Guid value)
    {
        Value = value;
    }

    public static SurveyId NewId() => new(Guid.NewGuid());

    public static SurveyId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SurveyId cannot be empty", nameof(value));

        return new SurveyId(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SurveyId id) => id.Value;
}
