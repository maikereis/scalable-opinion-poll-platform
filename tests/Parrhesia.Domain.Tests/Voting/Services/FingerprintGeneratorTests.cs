using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Services;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Tests.Voting.Services;

public class FingerprintGeneratorTests
{
    private const string SystemSalt = "test-salt-12345";

    [Fact]
    public void Generate_ShouldReturn64CharHex()
    {
        var generator = new FingerprintGenerator(SystemSalt);
        var userId = UserId.Create(Guid.NewGuid());
        var surveyId = SurveyId.NewId();

        var fingerprint = generator.Generate(userId, surveyId);

        fingerprint.Value.Should().HaveLength(64);
        fingerprint.Value.Should().MatchRegex("^[0-9A-F]{64}$");
    }

    [Fact]
    public void Generate_SameInputs_ShouldProduceSameFingerprint()
    {
        var generator = new FingerprintGenerator(SystemSalt);
        var userId = UserId.Create(Guid.NewGuid());
        var surveyId = SurveyId.NewId();

        var fp1 = generator.Generate(userId, surveyId);
        var fp2 = generator.Generate(userId, surveyId);

        fp1.Should().Be(fp2);
    }

    [Fact]
    public void Generate_DifferentUsers_ShouldProduceDifferentFingerprints()
    {
        var generator = new FingerprintGenerator(SystemSalt);
        var surveyId = SurveyId.NewId();
        var user1 = UserId.Create(Guid.NewGuid());
        var user2 = UserId.Create(Guid.NewGuid());

        var fp1 = generator.Generate(user1, surveyId);
        var fp2 = generator.Generate(user2, surveyId);

        fp1.Should().NotBe(fp2);
    }

    [Fact]
    public void Generate_DifferentSurveys_ShouldProduceDifferentFingerprints()
    {
        var generator = new FingerprintGenerator(SystemSalt);
        var userId = UserId.Create(Guid.NewGuid());
        var survey1 = SurveyId.NewId();
        var survey2 = SurveyId.NewId();

        var fp1 = generator.Generate(userId, survey1);
        var fp2 = generator.Generate(userId, survey2);

        fp1.Should().NotBe(fp2);
    }

    [Fact]
    public void Generate_DifferentSalts_ShouldProduceDifferentFingerprints()
    {
        var generator1 = new FingerprintGenerator("salt-1");
        var generator2 = new FingerprintGenerator("salt-2");
        var userId = UserId.Create(Guid.NewGuid());
        var surveyId = SurveyId.NewId();

        var fp1 = generator1.Generate(userId, surveyId);
        var fp2 = generator2.Generate(userId, surveyId);

        fp1.Should().NotBe(fp2);
    }

    [Fact]
    public void Constructor_WithEmptySalt_ShouldThrow()
    {
        Action act = () => new FingerprintGenerator("");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*System salt is required*");
    }

    [Fact]
    public void Generate_WithNullUserId_ShouldThrow()
    {
        var generator = new FingerprintGenerator(SystemSalt);

        Action act = () => generator.Generate(null!, SurveyId.NewId());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Generate_WithNullSurveyId_ShouldThrow()
    {
        var generator = new FingerprintGenerator(SystemSalt);
        var userId = UserId.Create(Guid.NewGuid());

        Action act = () => generator.Generate(userId, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
