using FluentAssertions;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Tests.Voting.ValueObjects;

public class VoterFingerprintTests
{
    private const string ValidFingerprint = "A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2";

    [Fact]
    public void Create_WithValidHex_ShouldSucceed()
    {
        var fingerprint = VoterFingerprint.Create(ValidFingerprint);

        fingerprint.Value.Should().Be(ValidFingerprint);
    }

    [Fact]
    public void Create_WithLowercaseHex_ShouldConvertToUppercase()
    {
        var lowercase = ValidFingerprint.ToLowerInvariant();

        var fingerprint = VoterFingerprint.Create(lowercase);

        fingerprint.Value.Should().Be(ValidFingerprint);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrow()
    {
        Action act = () => VoterFingerprint.Create("");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Create_WithWrongLength_ShouldThrow()
    {
        Action act = () => VoterFingerprint.Create("ABC123");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*exactly 64 characters*");
    }

    [Fact]
    public void Create_WithInvalidHexCharacters_ShouldThrow()
    {
        var invalidHex = new string('G', 64);

        Action act = () => VoterFingerprint.Create(invalidHex);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*valid hexadecimal*");
    }

    [Fact]
    public void TwoFingerprints_WithSameValue_ShouldBeEqual()
    {
        var fp1 = VoterFingerprint.Create(ValidFingerprint);
        var fp2 = VoterFingerprint.Create(ValidFingerprint);

        fp1.Should().Be(fp2);
    }
}
