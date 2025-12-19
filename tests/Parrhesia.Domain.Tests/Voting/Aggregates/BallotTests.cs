using FluentAssertions;
using Parrhesia.Domain.SurveyManagement.ValueObjects;
using Parrhesia.Domain.Voting.Aggregates;
using Parrhesia.Domain.Voting.Events;
using Parrhesia.Domain.Voting.ValueObjects;

namespace Parrhesia.Domain.Tests.Voting.Aggregates;

public class BallotTests
{
    private const string ValidFingerprint = "A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2C3D4E5F6A1B2";

    private static VoterFingerprint CreateFingerprint() => VoterFingerprint.Create(ValidFingerprint);

    [Fact]
    public void Cast_WithValidParameters_ShouldSucceed()
    {
        var surveyId = SurveyId.NewId();
        var questionId = QuestionId.NewId();
        var optionId = OptionId.NewId();
        var fingerprint = CreateFingerprint();

        var ballot = Ballot.Cast(surveyId, questionId, optionId, fingerprint);

        ballot.BallotId.Should().NotBeNull();
        ballot.SurveyId.Should().Be(surveyId);
        ballot.QuestionId.Should().Be(questionId);
        ballot.SelectedOptionId.Should().Be(optionId);
        ballot.VoterFingerprint.Should().Be(fingerprint);
        ballot.CastedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Cast_WithDeviceInfo_ShouldIncludeDeviceInfo()
    {
        var surveyId = SurveyId.NewId();
        var questionId = QuestionId.NewId();
        var optionId = OptionId.NewId();
        var fingerprint = CreateFingerprint();
        var deviceInfo = DeviceInfo.Create("device-123", "Mozilla", "192.168.1.1");

        var ballot = Ballot.Cast(surveyId, questionId, optionId, fingerprint, deviceInfo);

        ballot.DeviceInfo.Should().Be(deviceInfo);
    }

    [Fact]
    public void Cast_ShouldRaiseBallotCastedEvent()
    {
        var ballot = Ballot.Cast(
            SurveyId.NewId(),
            QuestionId.NewId(),
            OptionId.NewId(),
            CreateFingerprint());

        ballot.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BallotCastedEvent>();
    }

    [Fact]
    public void Cast_WithNullSurveyId_ShouldThrow()
    {
        Action act = () => Ballot.Cast(
            null!,
            QuestionId.NewId(),
            OptionId.NewId(),
            CreateFingerprint());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Cast_WithNullQuestionId_ShouldThrow()
    {
        Action act = () => Ballot.Cast(
            SurveyId.NewId(),
            null!,
            OptionId.NewId(),
            CreateFingerprint());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Cast_WithNullOptionId_ShouldThrow()
    {
        Action act = () => Ballot.Cast(
            SurveyId.NewId(),
            QuestionId.NewId(),
            null!,
            CreateFingerprint());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Cast_WithNullFingerprint_ShouldThrow()
    {
        Action act = () => Ballot.Cast(
            SurveyId.NewId(),
            QuestionId.NewId(),
            OptionId.NewId(),
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reconstitute_ShouldRestoreBallot()
    {
        var ballotId = BallotId.NewId();
        var surveyId = SurveyId.NewId();
        var questionId = QuestionId.NewId();
        var optionId = OptionId.NewId();
        var fingerprint = CreateFingerprint();
        var castedAt = DateTime.UtcNow.AddHours(-1);

        var ballot = Ballot.Reconstitute(
            ballotId, surveyId, questionId, optionId, fingerprint, castedAt);

        ballot.BallotId.Should().Be(ballotId);
        ballot.SurveyId.Should().Be(surveyId);
        ballot.CastedAt.Should().Be(castedAt);
        ballot.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Ballot_ShouldBeImmutable()
    {
        // Ballot should not expose public setters for its core properties
        // Note: With EF Core support, properties may have private setters for materialization,
        // but they should NOT be publicly settable after construction
        var type = typeof(Ballot);

        // Check that no properties have public setters
        var propertiesWithPublicSetters = type.GetProperties()
            .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true)
            .Select(p => p.Name)
            .ToList();

        propertiesWithPublicSetters.Should().BeEmpty(
            "Ballot should not have any properties with public setters. " +
            $"Found: {string.Join(", ", propertiesWithPublicSetters)}");
    }

    [Fact]
    public void Ballot_CoreProperties_ShouldBeGetOnly()
    {
        var ballot = Ballot.Cast(
            SurveyId.NewId(),
            QuestionId.NewId(),
            OptionId.NewId(),
            CreateFingerprint());

        var type = ballot.GetType();

        // These critical properties should be read-only (no public setter)
        var surveyIdProp = type.GetProperty(nameof(Ballot.SurveyId));
        var questionIdProp = type.GetProperty(nameof(Ballot.QuestionId));
        var optionIdProp = type.GetProperty(nameof(Ballot.SelectedOptionId));
        var fingerprintProp = type.GetProperty(nameof(Ballot.VoterFingerprint));
        var castedAtProp = type.GetProperty(nameof(Ballot.CastedAt));

        // Verify no public setters exist
        surveyIdProp!.SetMethod?.IsPublic.Should().NotBe(true, "SurveyId should not have a public setter");
        questionIdProp!.SetMethod?.IsPublic.Should().NotBe(true, "QuestionId should not have a public setter");
        optionIdProp!.SetMethod?.IsPublic.Should().NotBe(true, "SelectedOptionId should not have a public setter");
        fingerprintProp!.SetMethod?.IsPublic.Should().NotBe(true, "VoterFingerprint should not have a public setter");
        castedAtProp!.SetMethod?.IsPublic.Should().NotBe(true, "CastedAt should not have a public setter");
    }
}
