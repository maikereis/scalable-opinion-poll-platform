using System.Text.RegularExpressions;
using Parrhesia.Domain.Common;

namespace Parrhesia.Domain.Voting.ValueObjects;

public partial class VoterFingerprint : ValueObject
{
    private const int ExpectedLength = 64;

    public string Value { get; }

#pragma warning disable CS8618
    private VoterFingerprint() { }
#pragma warning restore CS8618

    private VoterFingerprint(string value)
    {
        Value = value;
    }

    public static VoterFingerprint Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Fingerprint cannot be empty", nameof(value));

        if (value.Length != ExpectedLength)
            throw new ArgumentException($"Fingerprint must be exactly {ExpectedLength} characters", nameof(value));

        if (!HexPattern().IsMatch(value))
            throw new ArgumentException("Fingerprint must be a valid hexadecimal string", nameof(value));

        return new VoterFingerprint(value.ToUpperInvariant());
    }

    public static VoterFingerprint Reconstitute(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[0-9A-Fa-f]{64}$")]
    private static partial Regex HexPattern();
}