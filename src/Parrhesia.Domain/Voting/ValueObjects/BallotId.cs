using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.Voting.ValueObjects;

public class BallotId : ValueObject
{
    public Guid Value { get; }

    private BallotId() { }

    private BallotId(Guid value)
    {
        Value = value;
    }

    public static BallotId NewId() => new(Guid.NewGuid());

    public static BallotId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("BallotId cannot be empty", nameof(value));

        return new BallotId(value);
    }

    public static BallotId Reconstitute(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(BallotId id) => id.Value;
}