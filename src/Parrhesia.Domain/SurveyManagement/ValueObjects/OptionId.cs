using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.SurveyManagement.ValueObjects;

public class OptionId : ValueObject
{
    public Guid Value { get; }

    private OptionId() { }

    private OptionId(Guid value)
    {
        Value = value;
    }

    public static OptionId NewId() => new(Guid.NewGuid());

    public static OptionId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OptionId cannot be empty", nameof(value));

        return new OptionId(value);
    }

    public static OptionId Reconstitute(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(OptionId id) => id.Value;
}